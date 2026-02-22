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
        private ILLManager _listManager;
        private IFSManager _fsManager;
        private string _productFilePath;
        private string _specFileName;

        public Repository(ILLManager listManager, IFSManager fsManager)
        {
            _fsManager = fsManager ?? throw new ArgumentNullException(nameof(fsManager));
            _listManager = listManager ?? throw new ArgumentNullException(nameof(listManager));
        }

        public void CreateProductFile(string filePath, short dataLength, string specFileName)
        {
            _productFilePath = filePath;
            _specFileName = specFileName;
            _fsManager.CreateProductFile();
            _header = new ProductHeader(dataLength, specFileName);
            _fsManager.WriteHeader(_header);
        }

        public void CreateSpecFile(string filePath, short dataLength, string specFileName) 
        {
        
        }

        public void Open(string filePath)
        {
            _filePath = filePath;
            ((ProductFSManager)_fsManager).OpenFile();

            _header = _fsManager.ReadHeader();
            _specFileName = _header.SpecFName;

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
            _listManager.Restore(productName);
        }

        public void RestoreAll()
        {
            _listManager.RestoreAll();
        }

        public void Truncate()
        {
            _listManager.Truncate();
            _listManager.SortAlphabetically();
        }

        public Product? Find(string productName)
        {
            return _listManager.FindByName(productName);
        }

        public IEnumerable<Product> GetAll()
        {
            return _listManager.GetAll();
        }
    }
}
