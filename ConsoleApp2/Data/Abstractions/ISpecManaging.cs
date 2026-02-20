using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data.Abstractions
{
    public interface ISpecManaging
    {
        void Create(string filePath);
        void Open(string filePath);
        void Close();
        void Add(Spec spec);
        void Delete(int specOffset);
        void Restore(int specOffset);
        void RestoreAll();
        void Truncate();
        Spec? FindByOffset(int offset);
        IEnumerable<Spec> GetByProductOffset(int productOffset);
        IEnumerable<Spec> GetByComponentPtr(int componentOffset);
    }
}
