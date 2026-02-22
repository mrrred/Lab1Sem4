using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ConsoleApp2.Data
{
    public class LLManager
    {
        private ProductFSManager _productFsManager;
        private SpecFSManager _specFsManager;
        private ProductSerializer _productSerializer;
        private SpecSerializer _specSerializer;
        private ProductHeader _productHeader;
        private Dictionary<string, Product> _productCache;
        private Dictionary<int, SpecHeader> _specHeaderCache;
        private Dictionary<int, List<Spec>> _specListCache;

        public LLManager(ProductFSManager productFsManager, SpecFSManager specFsManager, 
                         ProductSerializer productSerializer, SpecSerializer specSerializer)
        {
            _productFsManager = productFsManager ?? throw new ArgumentNullException(nameof(productFsManager));
            _specFsManager = specFsManager ?? throw new ArgumentNullException(nameof(specFsManager));
            _productSerializer = productSerializer ?? throw new ArgumentNullException(nameof(productSerializer));
            _specSerializer = specSerializer ?? throw new ArgumentNullException(nameof(specSerializer));
            _productCache = new Dictionary<string, Product>();
            _specHeaderCache = new Dictionary<int, SpecHeader>();
            _specListCache = new Dictionary<int, List<Spec>>();
        }

        public void Initialize(ProductHeader productHeader)
        {
            _productHeader = productHeader ?? throw new ArgumentNullException(nameof(productHeader));
            _productCache.Clear();
            _specHeaderCache.Clear();
            _specListCache.Clear();
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (_productCache.ContainsKey(product.Name))
                throw new InvalidOperationException("Component already exists: " + product.Name);

            int offset = (int)_productFsManager.GetStream().Length;
            product.FileOffset = offset;
            
            _productFsManager.Seek(offset);
            using (var writer = new BinaryWriter(_productFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
            {
                _productSerializer.WriteToFile(product, writer);
            }
            _productFsManager.GetStream().Flush();

            _productCache[product.Name] = product;
            
            _specHeaderCache[product.FileOffset] = new SpecHeader(-1, SpecHeader.GetHeaderSize());
            _specListCache[product.FileOffset] = new List<Spec>();
        }

        public void AddSpec(int productOffset, Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            if (!_specListCache.ContainsKey(productOffset))
                _specListCache[productOffset] = new List<Spec>();

            int offset = (int)_specFsManager.GetStream().Length;
            spec.FileOffset = offset;
            
            _specFsManager.Seek(offset);
            using (var writer = new BinaryWriter(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
            {
                _specSerializer.WriteToFile(spec, writer);
            }
            _specFsManager.GetStream().Flush();

            var specHeader = _specHeaderCache[productOffset];
            if (specHeader.FirstRecPtr == -1)
            {
                specHeader.FirstRecPtr = offset;
            }

            _specListCache[productOffset].Add(spec);
        }

        public void Delete(int offset)
        {
            var product = _productCache.Values.FirstOrDefault(p => p.FileOffset == offset);
            if (product != null)
            {
                product.MarkAsDeleted();
                UpdateProduct(product);
            }
        }

        public void DeleteSpec(int productOffset, int specOffset)
        {
            if (_specListCache.ContainsKey(productOffset))
            {
                var spec = _specListCache[productOffset].FirstOrDefault(s => s.FileOffset == specOffset);
                if (spec != null)
                {
                    spec.MarkAsDeleted();
                    UpdateSpec(spec);
                }
            }
        }

        public void Restore(string productName)
        {
            if (_productCache.TryGetValue(productName, out var product))
            {
                product.Restore();
                UpdateProduct(product);
            }
        }

        public void RestoreAllProducts()
        {
            foreach (var product in _productCache.Values)
            {
                if (product.IsDeleted)
                {
                    product.Restore();
                    UpdateProduct(product);
                }
            }
        }

        public void RestoreAllSpecs()
        {
            foreach (var specList in _specListCache.Values)
            {
                foreach (var spec in specList)
                {
                    if (spec.IsDeleted)
                    {
                        spec.Restore();
                        UpdateSpec(spec);
                    }
                }
            }
        }

        public void Truncate()
        {
            _productCache = _productCache
                .Where(p => !p.Value.IsDeleted)
                .ToDictionary(p => p.Key, p => p.Value);

            foreach (var key in _specListCache.Keys.ToList())
            {
                _specListCache[key] = _specListCache[key].Where(s => !s.IsDeleted).ToList();
            }
        }

        public Product FindByName(string name)
        {
            if (_productCache.TryGetValue(name, out var product) && !product.IsDeleted)
                return product;
            return null;
        }

        public Product FindByOffset(int offset)
        {
            return _productCache.Values.FirstOrDefault(p => p.FileOffset == offset && !p.IsDeleted);
        }

        public Spec FindSpecByOffset(int specOffset)
        {
            foreach (var specList in _specListCache.Values)
            {
                var spec = specList.FirstOrDefault(s => s.FileOffset == specOffset && !s.IsDeleted);
                if (spec != null)
                    return spec;
            }
            return null;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _productCache.Values.Where(p => !p.IsDeleted);
        }

        public Product GetLastProduct()
        {
            var allProducts = GetAllProducts().ToList();
            if (allProducts.Count == 0)
                return null;
            
            return allProducts.Last();
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.FileOffset == -1)
                throw new InvalidOperationException("Entity must have a valid FileOffset to update");

            _productFsManager.Seek(product.FileOffset);
            using (var writer = new BinaryWriter(_productFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
            {
                _productSerializer.WriteToFile(product, writer);
            }
            _productFsManager.GetStream().Flush();
        }

        public void UpdateSpec(Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            if (spec.FileOffset == -1)
                throw new InvalidOperationException("Entity must have a valid FileOffset to update");

            _specFsManager.Seek(spec.FileOffset);
            using (var writer = new BinaryWriter(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
            {
                _specSerializer.WriteToFile(spec, writer);
            }
            _specFsManager.GetStream().Flush();
        }

        public void LoadFromFile()
        {
            _productCache.Clear();
            _specHeaderCache.Clear();
            _specListCache.Clear();

            int currentOffset = _productHeader.FirstRecPtr;
            while (currentOffset != -1 && currentOffset != 1)
            {
                _productFsManager.Seek(currentOffset);
                using (var reader = new BinaryReader(_productFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
                {
                    var product = _productSerializer.ReadFromFile(reader);
                    product.FileOffset = currentOffset;
                    _productCache[product.Name] = product;
                    
                    if (!_specListCache.ContainsKey(product.FileOffset))
                    {
                        _specListCache[product.FileOffset] = new List<Spec>();
                    }
                    
                    currentOffset = product.NextProductPtr;
                }
            }

            foreach (var product in _productCache.Values.ToList())
            {
                if (product.SpecPtr != -1 && product.SpecPtr != 1)
                {
                    _specFsManager.Seek(product.SpecPtr);
                    using (var reader = new BinaryReader(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
                    {
                        var specHeader = new SpecHeader(reader.ReadInt32(), reader.ReadInt32());
                        _specHeaderCache[product.FileOffset] = specHeader;

                        int specOffset = specHeader.FirstRecPtr;
                        while (specOffset != -1 && specOffset != 1)
                        {
                            _specFsManager.Seek(specOffset);
                            using (var reader2 = new BinaryReader(_specFsManager.GetStream(), Encoding.UTF8, leaveOpen: true))
                            {
                                var spec = _specSerializer.ReadFromFile(reader2);
                                spec.FileOffset = specOffset;
                                
                                if (!_specListCache.ContainsKey(product.FileOffset))
                                {
                                    _specListCache[product.FileOffset] = new List<Spec>();
                                }
                                
                                _specListCache[product.FileOffset].Add(spec);
                                specOffset = spec.NextSpecPtr;
                            }
                        }
                    }
                }
                else if (!_specListCache.ContainsKey(product.FileOffset))
                {
                    _specListCache[product.FileOffset] = new List<Spec>();
                }
            }
        }

        public void SortAlphabetically()
        {
            var sortedProducts = _productCache.Values.OrderBy(p => p.Name).ToList();
            _productCache.Clear();
            foreach (var product in sortedProducts)
            {
                _productCache[product.Name] = product;
            }
        }

        public SpecHeader GetSpecHeader(int productOffset)
        {
            if (_specHeaderCache.ContainsKey(productOffset))
                return _specHeaderCache[productOffset];
            return null;
        }

        public IEnumerable<Spec> GetSpecsForProduct(int productOffset)
        {
            if (_specListCache.ContainsKey(productOffset))
                return _specListCache[productOffset];
            return Enumerable.Empty<Spec>();
        }
    }
}
