using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp2.Entities;

namespace ConsoleApp2.Data.Abstractions
{
    public interface IRepository
    {
        void Create(string filePath, short dataLength, string specFileName);
        void Open(string filePath);
        void Close();
        void Add(Product product);
        void Delete(string productName);
        void Restore(string productName);
        void RestoreAll();
        void Truncate();
        Product Find(string productName);
        IEnumerable<Product> GetAll();
    }
}
