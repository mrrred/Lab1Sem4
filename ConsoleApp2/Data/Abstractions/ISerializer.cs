using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data.Abstractions
{
    public interface ISerializer<T>
    {
        void WriteToFile(T entity, BinaryWriter writer);
        T ReadFromFile(BinaryReader reader);
        int GetEntitySize();
    }
}
