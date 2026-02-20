using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data.Abstractions
{
    public interface ILLManager<T> where T : class, IEntity
    {
        void Initialize(FileHeader header);
        void Add(T entity, int offset);
        void Delete(int offset);
        void Restore(int offset);
        void RestoreAll();
        void Truncate();
        T? FindByName(string name);
        T? FindByOffset(int offset);
        IEnumerable<T> GetAll();
        IEnumerable<T> FromOffset(int startOffset);
        void SortAlphabetically();
        void LoadFromFile();
    }
}
