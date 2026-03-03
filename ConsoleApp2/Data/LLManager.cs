using ConsoleApp2.Entities;
using ConsoleApp2.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

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
            _productHeader = productHeader ?? throw new ArgumentNullException(nameof(productHeader), "Product header is null.");
            _productCache.Clear();
            _specHeaderCache.Clear();
            _specListCache.Clear();
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product), "Product is null.");

            if (_productCache.ContainsKey(product.Name))
                throw new InvalidOperationException("Component already exists.");

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
            SortAlphabetically();
        }

        public void AddSpec(int productOffset, Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec), "Spec is null.");

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

            if (!_specHeaderCache.ContainsKey(productOffset))
            {
                var product = _productCache.Values.FirstOrDefault(p => p.FileOffset == productOffset);
                if (product != null && product.SpecPtr != -1)
                {
                    _specHeaderCache[productOffset] = new SpecHeader(-1, product.SpecPtr + SpecHeader.GetHeaderSize());
                }
                else if (product != null)
                {
                    _specHeaderCache[productOffset] = new SpecHeader(-1, SpecHeader.GetHeaderSize());
                }
            }

            var specHeader = _specHeaderCache[productOffset];
            if (specHeader != null && specHeader.FirstRecPtr == -1)
            {
                specHeader.FirstRecPtr = offset;
            }

            var comp = _productCache.Values.FirstOrDefault(p => p.FileOffset == spec.ComponentPtr);
            if (comp != null)
                spec.SetName(comp.Name);

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

                if (_specListCache.ContainsKey(product.FileOffset))
                {
                    foreach (var spec in _specListCache[product.FileOffset])
                    {
                        if (spec.IsDeleted)
                        {
                            spec.Restore();
                            UpdateSpec(spec);
                        }
                    }
                }
                SortAlphabetically();
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
            SortAlphabetically();
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

            foreach (var product in _productCache.Values)
            {
                if (_specListCache.ContainsKey(product.FileOffset))
                {
                    var specs = _specListCache[product.FileOffset].Where(s => !s.IsDeleted).ToList();
                    if (specs.Count > 0)
                    {
                        _specHeaderCache[product.FileOffset].FirstRecPtr = specs[0].FileOffset;
                    }
                }
            }

            SortAlphabetically();
        }

        public void Truncate()
        {
            var activeProducts = _productCache.Values
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.FileOffset)
                .ToList();

            if (activeProducts.Count == 0)
            {
                _productCache.Clear();
                _specHeaderCache.Clear();
                _specListCache.Clear();

                _productFsManager.CreateFile();
                _specFsManager.CreateFile();

                _productHeader.SetFirstRecPtr(-1);
                _productHeader.SetUnclaimedPtr(ProductHeader.GetHeaderSize());
                _productFsManager.WriteHeader(_productHeader);
                return;
            }

            int productHeaderSize = ProductHeader.GetHeaderSize();
            int productEntitySize = _productSerializer.GetEntitySize();
            int specEntitySize = _specSerializer.GetEntitySize();
            var oldProductOffsets = activeProducts.Select(p => p.FileOffset).ToList();

            var tempProductStream = _productFsManager.CreateTempFile();
            var tempSpecStream = _specFsManager.CreateTempFile();

            try
            {
                using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                {
                    _productHeader.SetFirstRecPtr((activeProducts.Count > 0) ? productHeaderSize : -1);

                    writer.Write(Encoding.ASCII.GetBytes(_productHeader.Signature.PadRight(2).Substring(0, 2)));
                    writer.Write(_productHeader.DataLength);
                    writer.Write(_productHeader.FirstRecPtr);
                    writer.Write(0); 

                    if (!string.IsNullOrEmpty(_productHeader.SpecFileName))
                    {
                        char[] specBuffer = new char[16];
                        Array.Copy(_productHeader.SpecFileName.ToCharArray(), specBuffer,
                            Math.Min(_productHeader.SpecFileName.Length, 16));
                        writer.Write(specBuffer);
                    }

                    for (int i = 0; i < activeProducts.Count; i++)
                    {
                        int newOffset = (int)tempProductStream.Position;
                        activeProducts[i].SetFileOffset(newOffset);

                        _productSerializer.WriteToFile(activeProducts[i], writer);

                        int newUnclaimedPtr = (int)tempProductStream.Position;
                        _productHeader.SetUnclaimedPtr(newUnclaimedPtr);
                    }
                }

                using (var writer = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                {
                    for (int i = 0; i < activeProducts.Count; i++)
                    {
                        int oldProductOffset = oldProductOffsets[i];

                        if (_specListCache.TryGetValue(oldProductOffset, out var specs))
                        {
                            var activeSpecs = specs
                                .Where(s => !s.IsDeleted)
                                .ToList();

                            if (activeSpecs.Count > 0)
                            {
                                int specHeaderOffset = (int)tempSpecStream.Position;
                                activeProducts[i].SetSpecPtr(specHeaderOffset);

                                tempSpecStream.Seek(0, SeekOrigin.End);
                                int headerPos = (int)tempSpecStream.Position;

                                using (var headerWriter = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                                {
                                    int firstSpecPos = headerPos + SpecHeader.GetHeaderSize();
                                    headerWriter.Write(firstSpecPos);

                                    headerWriter.Write(0);
                                }

                                int unclaimedPtrPos = headerPos + FileStructure.POINTER_SIZE;

                                int lastSpecOffset = 0;
                                for (int j = 0; j < activeSpecs.Count; j++)
                                {
                                    int newSpecOffset = (int)tempSpecStream.Position;
                                    activeSpecs[j].SetFileOffset(newSpecOffset);
                                    activeSpecs[j].SetNextSpecPtr((j < activeSpecs.Count - 1)
                                        ? newSpecOffset + specEntitySize
                                        : -1);

                                    _specSerializer.WriteToFile(activeSpecs[j], writer);
                                    lastSpecOffset = (int)tempSpecStream.Position;

                                    _productHeader.SetUnclaimedPtr(lastSpecOffset);
                                }

                                tempSpecStream.Seek(unclaimedPtrPos, SeekOrigin.Begin);
                                using (var headerWriter = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                                {
                                    headerWriter.Write(lastSpecOffset);
                                }
                                tempSpecStream.Seek(0, SeekOrigin.End);
                            }
                            else
                            {
                                activeProducts[i].SetSpecPtr(-1);
                            }
                        }
                        else
                        {
                            activeProducts[i].SetSpecPtr(-1);
                        }
                    }
                }

                for (int i = 0; i < activeProducts.Count; i++)
                {
                    activeProducts[i].SetNextProductPtr((i < activeProducts.Count - 1)
                        ? activeProducts[i].FileOffset + productEntitySize
                        : -1);
                }

                tempProductStream.Seek(0, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(Encoding.ASCII.GetBytes(_productHeader.Signature.PadRight(2).Substring(0, 2)));
                    writer.Write(_productHeader.DataLength);
                    writer.Write(_productHeader.FirstRecPtr);
                    writer.Write(_productHeader.UnclaimedPtr);

                    if (!string.IsNullOrEmpty(_productHeader.SpecFileName))
                    {
                        char[] specBuffer = new char[16];
                        Array.Copy(_productHeader.SpecFileName.ToCharArray(), specBuffer,
                            Math.Min(_productHeader.SpecFileName.Length, 16));
                        writer.Write(specBuffer);
                    }
                }

                for (int i = 0; i < activeProducts.Count; i++)
                {
                    tempProductStream.Seek(activeProducts[i].FileOffset, SeekOrigin.Begin);
                    using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                    {
                        _productSerializer.WriteToFile(activeProducts[i], writer);
                    }
                }

                tempProductStream.Flush();
                tempSpecStream.Flush();
            }
            catch
            {
                if (File.Exists(_productFsManager.GetTempFilePath()))
                    File.Delete(_productFsManager.GetTempFilePath());
                if (File.Exists(_specFsManager.GetTempFilePath()))
                    File.Delete(_specFsManager.GetTempFilePath());
                throw;
            }
            finally
            {
                tempProductStream?.Dispose();
                tempSpecStream?.Dispose();
            }

            _productFsManager.ReplaceWithTempFile();
            _specFsManager.ReplaceWithTempFile();

            _productCache = activeProducts.ToDictionary(p => p.Name, p => p);

            var newSpecListCache = new Dictionary<int, List<Spec>>();
            for (int i = 0; i < activeProducts.Count; i++)
            {
                int oldProductOffset = oldProductOffsets[i];
                if (_specListCache.TryGetValue(oldProductOffset, out var specs))
                {
                    var activeSpecs = specs.Where(s => !s.IsDeleted).ToList();
                    if (activeSpecs.Count > 0)
                    {
                        newSpecListCache[activeProducts[i].FileOffset] = activeSpecs;
                    }
                }
            }
            _specListCache = newSpecListCache;

            _specHeaderCache.Clear();
            foreach (var product in activeProducts)
            {
                if (_specListCache.ContainsKey(product.FileOffset))
                {
                    var specs = _specListCache[product.FileOffset];
                    int firstSpecPtr = specs.Count > 0 ? specs[0].FileOffset : -1;
                    int unclaimedPtr = specs.Count > 0 ? specs[specs.Count - 1].FileOffset + specEntitySize : SpecHeader.GetHeaderSize();
                    _specHeaderCache[product.FileOffset] = new SpecHeader(firstSpecPtr, unclaimedPtr);
                }
            }
        }

        public Product FindByName(string name)
        {
            if (_productCache.TryGetValue(name, out var product) && !product.IsDeleted)
                return product;
            return null;
        }

        public Product FindByNameIncludeDeleted(string name)
        {
            if (_productCache.TryGetValue(name, out var product))
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
                throw new ArgumentNullException(nameof(product), "Product is null.");

            if (product.FileOffset == -1)
                throw new InvalidOperationException("Product must have valid offset.");

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
                throw new ArgumentNullException(nameof(spec), "Spec is null.");

            if (spec.FileOffset == -1)
                throw new InvalidOperationException("Spec must have valid offset.");

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

                                var comp = _productCache.Values.FirstOrDefault(p => p.FileOffset == spec.ComponentPtr);
                                if (comp != null)
                                    spec.SetName(comp.Name);

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

            SortAlphabetically();
        }

        public void SortAlphabetically()
        {
            var sortedProducts = _productCache.Values
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToList();

            if (sortedProducts.Count == 0)
            {
                _productCache.Clear();
                _specHeaderCache.Clear();
                _specListCache.Clear();

                _productFsManager.CreateFile();
                _specFsManager.CreateFile();

                _productHeader.SetFirstRecPtr(-1);
                _productHeader.SetUnclaimedPtr(ProductHeader.GetHeaderSize());
                _productFsManager.WriteHeader(_productHeader);

                return;
            }

            var oldProductOffsets = sortedProducts.Select(p => p.FileOffset).ToList();

            int productHeaderSize = ProductHeader.GetHeaderSize();
            int productEntitySize = _productSerializer.GetEntitySize();
            int specEntitySize = _specSerializer.GetEntitySize();

            var tempProductStream = _productFsManager.CreateTempFile();
            var tempSpecStream = _specFsManager.CreateTempFile();

            try
            {
                using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                {
                    _productHeader.SetFirstRecPtr((sortedProducts.Count > 0) ? productHeaderSize : -1);

                    writer.Write(Encoding.ASCII.GetBytes(_productHeader.Signature.PadRight(2).Substring(0, 2)));
                    writer.Write(_productHeader.DataLength);
                    writer.Write(_productHeader.FirstRecPtr);
                    writer.Write(0); 

                    if (!string.IsNullOrEmpty(_productHeader.SpecFileName))
                    {
                        char[] specBuffer = new char[16];
                        Array.Copy(_productHeader.SpecFileName.ToCharArray(), specBuffer,
                            Math.Min(_productHeader.SpecFileName.Length, 16));
                        writer.Write(specBuffer);
                    }

                    for (int i = 0; i < sortedProducts.Count; i++)
                    {
                        int newOffset = (int)tempProductStream.Position;
                        sortedProducts[i].SetFileOffset(newOffset);

                        _productSerializer.WriteToFile(sortedProducts[i], writer);
                        int newUnclaimedPtr = (int)tempProductStream.Position;
                        _productHeader.SetUnclaimedPtr(newUnclaimedPtr);
                    }
                }

                using (var writer = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                {
                    for (int i = 0; i < sortedProducts.Count; i++)
                    {
                        int oldProductOffset = oldProductOffsets[i];

                        if (_specListCache.TryGetValue(oldProductOffset, out var specs))
                        {
                            var activeSpecs = specs
                                .Where(s => !s.IsDeleted)
                                .ToList();

                            if (activeSpecs.Count > 0)
                            {
                                int specHeaderOffset = (int)tempSpecStream.Position;
                                sortedProducts[i].SetSpecPtr(specHeaderOffset);

                                tempSpecStream.Seek(0, SeekOrigin.End);
                                int headerPos = (int)tempSpecStream.Position;

                                using (var headerWriter = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                                {
                                    int firstSpecPos = headerPos + SpecHeader.GetHeaderSize();
                                    headerWriter.Write(firstSpecPos);

                                    headerWriter.Write(0);
                                }

                                int unclaimedPtrPos = headerPos + FileStructure.POINTER_SIZE;

                                int lastSpecOffset = 0;
                                for (int j = 0; j < activeSpecs.Count; j++)
                                {
                                    int newSpecOffset = (int)tempSpecStream.Position;
                                    activeSpecs[j].SetFileOffset(newSpecOffset);
                                    activeSpecs[j].SetNextSpecPtr((j < activeSpecs.Count - 1)
                                        ? newSpecOffset + specEntitySize
                                        : -1);

                                    _specSerializer.WriteToFile(activeSpecs[j], writer);
                                    lastSpecOffset = (int)tempSpecStream.Position;
                                    int newUnclaimedPtr = (int)tempSpecStream.Position;
                                    _productHeader.SetUnclaimedPtr(newUnclaimedPtr);
                                }

                                tempSpecStream.Seek(unclaimedPtrPos, SeekOrigin.Begin);
                                using (var headerWriter = new BinaryWriter(tempSpecStream, Encoding.UTF8, leaveOpen: true))
                                {
                                    headerWriter.Write(lastSpecOffset);
                                }
                                tempSpecStream.Seek(0, SeekOrigin.End);
                            }
                            else
                            {
                                sortedProducts[i].SetSpecPtr(-1);
                            }
                        }
                        else
                        {
                            sortedProducts[i].SetSpecPtr(-1);
                        }
                    }
                }
                for (int i = 0; i < sortedProducts.Count; i++)
                {
                    sortedProducts[i].SetNextProductPtr((i < sortedProducts.Count - 1)
                        ? sortedProducts[i].FileOffset + productEntitySize
                        : -1);
                }

                tempProductStream.Seek(0, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(Encoding.ASCII.GetBytes(_productHeader.Signature.PadRight(2).Substring(0, 2)));
                    writer.Write(_productHeader.DataLength);
                    writer.Write(_productHeader.FirstRecPtr);
                    writer.Write(_productHeader.UnclaimedPtr);

                    if (!string.IsNullOrEmpty(_productHeader.SpecFileName))
                    {
                        char[] specBuffer = new char[16];
                        Array.Copy(_productHeader.SpecFileName.ToCharArray(), specBuffer,
                            Math.Min(_productHeader.SpecFileName.Length, 16));
                        writer.Write(specBuffer);
                    }
                }

                for (int i = 0; i < sortedProducts.Count; i++)
                {
                    tempProductStream.Seek(sortedProducts[i].FileOffset, SeekOrigin.Begin);
                    using (var writer = new BinaryWriter(tempProductStream, Encoding.UTF8, leaveOpen: true))
                    {
                        _productSerializer.WriteToFile(sortedProducts[i], writer);
                    }
                }

                tempProductStream.Flush();
                tempSpecStream.Flush();
            }
            catch
            {
                if (File.Exists(_productFsManager.GetTempFilePath()))
                    File.Delete(_productFsManager.GetTempFilePath());
                if (File.Exists(_specFsManager.GetTempFilePath()))
                    File.Delete(_specFsManager.GetTempFilePath());
                throw;
            }
            finally
            {
                tempProductStream?.Dispose();
                tempSpecStream?.Dispose();
            }

            _productFsManager.ReplaceWithTempFile();
            _specFsManager.ReplaceWithTempFile();

            _productCache = sortedProducts.ToDictionary(p => p.Name, p => p);

            var newSpecListCache = new Dictionary<int, List<Spec>>();
            for (int i = 0; i < sortedProducts.Count; i++)
            {
                int oldProductOffset = oldProductOffsets[i];
                if (_specListCache.TryGetValue(oldProductOffset, out var specs))
                {
                    var activeSpecs = specs.Where(s => !s.IsDeleted).ToList();
                    if (activeSpecs.Count > 0)
                    {
                        newSpecListCache[sortedProducts[i].FileOffset] = activeSpecs;
                    }
                }
            }
            _specListCache = newSpecListCache;

            _specHeaderCache.Clear();
            foreach (var product in sortedProducts)
            {
                if (_specListCache.ContainsKey(product.FileOffset))
                {
                    var specs = _specListCache[product.FileOffset];
                    int firstSpecPtr = specs.Count > 0 ? specs[0].FileOffset : -1;
                    int unclaimedPtr = specs.Count > 0 ? specs[specs.Count - 1].FileOffset + specEntitySize : SpecHeader.GetHeaderSize();
                    _specHeaderCache[product.FileOffset] = new SpecHeader(firstSpecPtr, unclaimedPtr);
                }
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

        public void EditProduct(string oldName, string newName)
        {
            if (!_productCache.TryGetValue(oldName, out var product))
                throw new InvalidOperationException("Component not found.");

            if (oldName != newName && _productCache.ContainsKey(newName))
                throw new InvalidOperationException("Component with this name already exists.");

            product.SetComponentName(newName);
            UpdateProduct(product);

            _productCache.Remove(oldName);
            _productCache[newName] = product;
            SortAlphabetically();
        }

        public void EditSpec(int productOffset, int specComponentPtr, ushort newMultiplicity)
        {
            if (!_specListCache.ContainsKey(productOffset))
                throw new InvalidOperationException("Product specifications not found.");

            var spec = _specListCache[productOffset]
                .FirstOrDefault(s => s.ComponentPtr == specComponentPtr && !s.IsDeleted);

            if (spec == null)
                throw new InvalidOperationException("Specification not found.");

            spec.SetMultiplicity(newMultiplicity);
            UpdateSpec(spec);
        }
    }
}
