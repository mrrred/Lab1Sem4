using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;

namespace ConsoleApp2.MenuService
{
    public interface IFileService
    {
        void Create(string fileName);
        void Open(string fileName);
        void Input(string componentName, ComponentType type);
        void Input(string componentName, string specificationName);
        void Input(string componentName, string specificationName, ushort multiplicity);
        void Delete(string componentName);
        void Delete(string componentName, string specificationName);
        void Restore(string componentName);
        void Restore();
        void Truncate();
        void Print(string componentName);
        void Print();
        void Help();
        void Help(string fileName);

        IEnumerable<Product> GetAllProducts();
        Product GetProduct(string productName);
        IEnumerable<Spec> GetProductSpecifications(string productName);
        bool IsFilesOpen { get; }

        event EventHandler ProductsChanged;
        event EventHandler<string> ErrorOccurred;
    }
}
