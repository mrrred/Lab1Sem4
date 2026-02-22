using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp2.Entities;

namespace ConsoleApp2.Data.Abstractions
{
    public interface ILLManager
    {
        void Initialize(ProductHeader header);
        void AddProduct();
        void AddSpec();
        void Delete(int offset);
        void RestoreProduct(Product product);
        void RestoreSpec();
        void RestoreAll();
        void Truncate();
        Product FindProduct(string name);
        Spec FindSpec(string prod_name, string spec_name);
        void SortAlphabetically();
        void LoadFromFile();
        void Update();
    }
}
