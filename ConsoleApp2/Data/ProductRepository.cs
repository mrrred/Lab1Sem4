using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ConsoleApp2.Data
{
    public class ProductRepository : IProductManaging
    {
        private IFSManager _fsManager;
        private ISerializer<Product> _serializer;
        private ILLManager<Product> _listManager;
        private INodeNavigator<Product> _navigator;
        private FileHeader _header;
        private string _filePath;
        private string _specFileName;

        public ProductRepository(
            IFSManager fsManager,
            ISerializer<Product> serializer,
            ILLManager<Product> listManager,
            INodeNavigator<Product> navigator)
        {
            _fsManager = fsManager ?? throw new ArgumentNullException(nameof(fsManager));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _listManager = listManager ?? throw new ArgumentNullException(nameof(listManager));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        public void Create(string filePath, short dataLength, string specFileName)
        {
            _filePath = filePath;
            _specFileName = specFileName;

            ((FSManager)_fsManager).CreateFile();

            _header = new FileHeader
            {
                Signature = "PS",
                DataLength = dataLength,
                FirstRecPtr = -1,
                UnclaimedPtr = -1,
                SpecFName = specFileName
            };

            _fsManager.WriteHeader(_header);
            _listManager.Initialize(_header);
            _navigator = new ProductNodeNavigator(_header);
        }

        public void Open(string filePath)
        {
            _filePath = filePath;
            ((FSManager)_fsManager).OpenFile();

            _header = _fsManager.ReadHeader();
            _specFileName = _header.SpecFName;

            _listManager.Initialize(_header);
            _navigator = new ProductNodeNavigator(_header);
            _listManager.LoadFromFile();
        }

        public void Close()
        {
            _fsManager.Close();
        }

        public void Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (_listManager.FindByName(product.Name) != null)
                throw new InvalidOperationException("Component already exists: " + product.Name);

            int offset = CalculateNextOffset();
            _listManager.Add(product, offset);

            if (_header.FirstRecPtr == -1)
            {
                _header.FirstRecPtr = offset;
                _navigator.SetFirstOffset(offset);
            }

            UpdateHeader();
        }

        public void Delete(string productName)
        {
            var product = _listManager.FindByName(productName);
            if (product == null)
                throw new InvalidOperationException("Component not found: " + productName);

            _listManager.Delete(product.FileOffset);
            
            int deletedOffset = product.FileOffset;
            _header.UnclaimedPtr = deletedOffset;
            
            UpdateHeader();
        }

        public void Restore(string productName)
        {
            var product = _listManager.FindByName(productName);
            if (product == null)
                throw new InvalidOperationException("Component not found: " + productName);

            _listManager.Restore(product.FileOffset);
            UpdateHeader();
        }

        public void RestoreAll()
        {
            _listManager.RestoreAll();
            _listManager.SortAlphabetically();
            UpdateHeader();
        }

        public void Truncate()
        {
            _listManager.Truncate();
            CompactFile();
            _listManager.SortAlphabetically();
            UpdateHeader();
        }

        public Product? Find(string productName)
        {
            return _listManager.FindByName(productName);
        }

        public IEnumerable<Product> GetAll()
        {
            return _listManager.GetAll();
        }

        private void UpdateHeader()
        {
            _fsManager.WriteHeader(_header);
        }

        private int CalculateNextOffset()
        {
            if (_header.UnclaimedPtr != -1)
            {
                int targetOffset = _header.UnclaimedPtr;
                
                _fsManager.Seek(targetOffset);
                using (var reader = new BinaryReader(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    reader.ReadByte();
                    int nextUnclaimed = reader.ReadInt32();
                    _header.UnclaimedPtr = nextUnclaimed;
                }
                
                return targetOffset;
            }
            
            long fileSize = _fsManager.GetStream().Length;
            if (fileSize == 0)
                return FileStructure.HEADER_SIZE_PRODUCT;
            
            return (int)fileSize;
        }

        private void CompactFile()
        {
            var activeProducts = _listManager.GetAll().OrderBy(p => p.Name).ToList();
            
            if (activeProducts.Count == 0)
            {
                _header.FirstRecPtr = -1;
                _header.UnclaimedPtr = -1;
                return;
            }
            
            string tempPath = _filePath + ".tmp";
            
            using (var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite))
            {
                int currentOffset = FileStructure.HEADER_SIZE_PRODUCT;
                int firstOffset = currentOffset;
                
                for (int i = 0; i < activeProducts.Count; i++)
                {
                    var product = activeProducts[i];
                    product.FileOffset = currentOffset;
                    
                    if (i < activeProducts.Count - 1)
                    {
                        product.NextProductPtr = currentOffset + _serializer.GetEntitySize();
                    }
                    else
                    {
                        product.NextProductPtr = -1;
                    }
                    
                    _fsManager.Seek(currentOffset);
                    using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                    {
                        _serializer.WriteToFile(product, writer);
                    }
                    
                    currentOffset += _serializer.GetEntitySize();
                }
                
                _header.FirstRecPtr = firstOffset;
                _header.UnclaimedPtr = -1;
                
                _fsManager.Seek(0);
                using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    _fsManager.WriteHeader(_header);
                }
            }
        }
    }
}
