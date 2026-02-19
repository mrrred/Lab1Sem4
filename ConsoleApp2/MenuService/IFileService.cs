using ConsoleApp21;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.MenuService
{


    interface IFileService
    {
        void Create(string fileName);

        void Open(string fileName);

        void Input(string componentName, ComponentType type);

        void Input(string componentName, string specificationName);

        void Delete(string componentName);

        void Delete(string componentName, string specificationName);

        void Restore(string componentName);

        void Restore();

        void Truncate();

        void Print(string componentName);

        void Print();

        void Help();

        void Help(string fileName);

    }

    enum ComponentType
    {
        Product, Unit, Detail
    }
}
