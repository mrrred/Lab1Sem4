using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ConsoleApp2.Data
{
    public class Repository : IRepository
    {
        private ProductFSManager _productFsManager;
        private SpecFSManager _specFsManager;
        private LLManager _listManager;
        private ProductHeader _productHeader;
        private ProductSerializer _productSerializer;
        private SpecSerializer _specSerializer;

        public Repository(ProductFSManager productFsManager, SpecFSManager specFsManager, 
                         ProductSerializer productSerializer, SpecSerializer specSerializer)
        {
            _productFsManager = productFsManager ?? throw new ArgumentNullException(nameof(productFsManager));
            _specFsManager = specFsManager ?? throw new ArgumentNullException(nameof(specFsManager));
            _productSerializer = productSerializer ?? throw new ArgumentNullException(nameof(productSerializer));
            _specSerializer = specSerializer ?? throw new ArgumentNullException(nameof(specSerializer));
            _listManager = new LLManager(productFsManager, specFsManager, productSerializer, specSerializer);
        }

        public void Create(string filePath, short dataLength, string specFileName)
        {
            if (!filePath.EndsWith(".prd"))
                filePath += ".prd";
            
            string dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir))
                dir = ".";
            
            string specFilePath = Path.Combine(dir, specFileName);

            _productFsManager.CreateFile();
            _specFsManager.CreateFile();

            _productHeader = new ProductHeader(dataLength, Path.GetFileName(specFileName));
            _productHeader.FirstRecPtr = -1;
            _productHeader.UnclaimedPtr = ProductHeader.GetHeaderSize();

            _listManager.Initialize(_productHeader);

            _productFsManager.WriteHeader(_productHeader);
        }

        public void Open(string filePath)
        {
            if (!filePath.EndsWith(".prd"))
                filePath += ".prd";

            if (_productFsManager.GetStream() == null)
            {
                _productFsManager.OpenFile();
            }

            _productHeader = _productFsManager.ReadHeader();
            
            string dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir))
                dir = ".";
            
            string specFilePath = Path.Combine(dir, _productHeader.SpecFileName);

            if (_specFsManager.GetStream() == null)
            {
                _specFsManager.OpenFile();
            }

            _listManager.Initialize(_productHeader);
            _listManager.LoadFromFile();
        }

        public void Close()
        {
            _productFsManager?.Close();
            _specFsManager?.Close();
        }

        public void Save()
        {
            if (_productHeader != null)
                _productFsManager.WriteHeader(_productHeader);
        }

        public void Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (_listManager == null)
                throw new InvalidOperationException("Repository not initialized. Call Create or Open first.");

            if (_productFsManager == null || _productFsManager.GetStream() == null)
                throw new InvalidOperationException("Product file not open");

            if (_specFsManager == null || _specFsManager.GetStream() == null)
                throw new InvalidOperationException("Spec file not open");

            if (_listManager.FindByName(product.Name) != null)
                throw new InvalidOperationException("Component already exists: " + product.Name);

            int headerSize = ProductHeader.GetHeaderSize();
            int offset = (int)_productFsManager.GetStream().Length;
            
            if (offset < headerSize)
                offset = headerSize;

            product.FileOffset = offset;

            int specHeaderOffset = (int)_specFsManager.GetStream().Length;
            if (specHeaderOffset == 0)
                specHeaderOffset = 0;

            product.SpecPtr = specHeaderOffset;

            if (_productHeader.FirstRecPtr == -1)
            {
                _productHeader.FirstRecPtr = offset;
            }
            else
            {
                var lastProduct = _listManager.GetLastProduct();
                if (lastProduct != null)
                {
                    lastProduct.NextProductPtr = offset;
                    _listManager.UpdateProduct(lastProduct);
                }
            }

            _listManager.AddProduct(product);

            _specFsManager.Seek(specHeaderOffset);
            var specHeader = new SpecHeader(-1, specHeaderOffset + SpecHeader.GetHeaderSize());
            using (var writer = new BinaryWriter(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(specHeader.FirstRecPtr);
                writer.Write(specHeader.UnclaimedPtr);
            }
            _specFsManager.GetStream().Flush();

            _listManager.UpdateProduct(product);
            _productFsManager.WriteHeader(_productHeader);
        }

        public void Delete(string productName)
        {
            var product = _listManager.FindByName(productName);
            if (product == null)
                throw new InvalidOperationException("Component not found: " + productName);

            var specs = _listManager.GetSpecsForProduct(product.FileOffset).Where(s => !s.IsDeleted).ToList();
            if (specs.Count > 0)
                throw new InvalidOperationException("Component is referenced in specifications");

            _listManager.Delete(product.FileOffset);
        }

        public void Restore(string productName)
        {
            _listManager.Restore(productName);
            _listManager.SortAlphabetically();
        }

        public void RestoreAll()
        {
            _listManager.RestoreAllProducts();
            _listManager.RestoreAllSpecs();
            _listManager.SortAlphabetically();
        }

        public void Truncate()
        {
            _listManager.Truncate();
        }

        public Product Find(string productName)
        {
            return _listManager.FindByName(productName);
        }

        public IEnumerable<Product> GetAll()
        {
            return _listManager.GetAllProducts();
        }

        public void AddSpec(Product product, Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            int specOffset = (int)_specFsManager.GetStream().Length;

            spec.FileOffset = specOffset;

            var specHeader = _listManager.GetSpecHeader(product.FileOffset);
            if (specHeader != null && specHeader.FirstRecPtr != -1)
            {
                int currentOffset = specHeader.FirstRecPtr;
                Spec lastSpec = null;

                while (currentOffset != -1 && currentOffset != 1)
                {
                    var s = _listManager.FindSpecByOffset(currentOffset);
                    if (s == null)
                        break;

                    lastSpec = s;
                    currentOffset = s.NextSpecPtr;
                }

                if (lastSpec != null)
                {
                    lastSpec.NextSpecPtr = specOffset;
                    _listManager.UpdateSpec(lastSpec);
                }
            }
            else
            {
                specHeader.FirstRecPtr = specOffset;
                _specFsManager.Seek(product.SpecPtr);
                using (var writer = new BinaryWriter(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(specHeader.FirstRecPtr);
                    writer.Write(specHeader.UnclaimedPtr);
                }
                _specFsManager.GetStream().Flush();
            }

            _listManager.AddSpec(product.FileOffset, spec);
        }

        public void DeleteSpec(Product product, string specName)
        {
            var specComponent = _listManager.FindByName(specName);
            if (specComponent == null)
                throw new InvalidOperationException("Component not found: " + specName);

            var specHeader = _listManager.GetSpecHeader(product.FileOffset);
            if (specHeader == null)
                throw new InvalidOperationException("Specification not found");

            int currentOffset = specHeader.FirstRecPtr;
            Spec specToDelete = null;
            Spec prevSpec = null;

            while (currentOffset != -1 && currentOffset != 1)
            {
                var spec = _listManager.FindSpecByOffset(currentOffset);
                if (spec == null)
                    break;

                if (spec.ComponentPtr == specComponent.FileOffset)
                {
                    specToDelete = spec;
                    break;
                }

                prevSpec = spec;
                currentOffset = spec.NextSpecPtr;
            }

            if (specToDelete == null)
                throw new InvalidOperationException("Specification not found");

            specToDelete.MarkAsDeleted();
            _listManager.UpdateSpec(specToDelete);

            if (prevSpec != null)
            {
                prevSpec.NextSpecPtr = specToDelete.NextSpecPtr;
                _listManager.UpdateSpec(prevSpec);
            }
            else if (specHeader.FirstRecPtr == specToDelete.FileOffset)
            {
                specHeader.FirstRecPtr = specToDelete.NextSpecPtr;
                _specFsManager.Seek(product.SpecPtr);
                using (var writer = new BinaryWriter(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(specHeader.FirstRecPtr);
                    writer.Write(specHeader.UnclaimedPtr);
                }
                _specFsManager.GetStream().Flush();
            }
        }

        public Spec FindSpecByOffset(int offset)
        {
            return _listManager.FindSpecByOffset(offset);
        }

        public IEnumerable<Spec> GetAllSpecs()
        {
            return Enumerable.Empty<Spec>();
        }

        public IEnumerable<Spec> GetSpecsForProduct(int productOffset)
        {
            return _listManager.GetSpecsForProduct(productOffset);
        }
    }
}
