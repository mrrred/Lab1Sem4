using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data.Abstractions
{
    public interface IProductManaging
    {
        void Create(string filePath, short dataLength, string specFileName);
        void Open(string filePath);
        void Close();
        void Add(Product product);
        void Delete(string productName);
        void Restore(string productName);
        void RestoreAll();
        void Truncate();
        Product? Find(string productName);
        IEnumerable<Product> GetAll();
    }
}
