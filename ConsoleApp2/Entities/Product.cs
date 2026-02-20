using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Entities
{
    public enum ComponentType
    {
        Product,
        Node,
        Detail
    }

    public interface IDeletable
    {
        byte DelBit { get; set; }
        void MarkAsDeleted();
        void Restore();
        bool IsDeleted { get; }
    }

    public interface IEntity : IDeletable
    {
        string Name { get; }
        int FileOffset { get; set; }
    }

    public class Product : IEntity
    {
        public byte DelBit { get; set; }
        public int SpecPtr { get; set; }
        public int NextProductPtr { get; set; }
        public string Name { get; private set; }
        public ComponentType Type { get; set; }
        public int FileOffset { get; set; }
        public bool IsDeleted => DelBit == 1;

        public Product(string name, ComponentType type)
        {
            DelBit = 0;
            SpecPtr = -1;
            NextProductPtr = -1;
            Name = name;
            Type = type;
            FileOffset = -1;
        }

        public void MarkAsDeleted() => DelBit = 1;
        public void Restore() => DelBit = 0;
    }
}
