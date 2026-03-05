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
        byte DelBit { get; }
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
        private byte _delBit;
        private int _specPtr;
        private int _nextProductPtr;
        private int _fileOffset;

        public byte DelBit
        {
            get { return _delBit; }
            set { _delBit = value; }
        }

        public int SpecPtr
        {
            get { return _specPtr; }
            set { _specPtr = value; }
        }

        public int NextProductPtr
        {
            get { return _nextProductPtr; }
            set { _nextProductPtr = value; }
        }

        public string Name { get; private set; }

        public ComponentType Type { get; private set; }

        public int FileOffset
        {
            get { return _fileOffset; }
            set { _fileOffset = value; }
        }

        public bool IsDeleted => DelBit == 1;

        public Product(string name, ComponentType type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Component name cannot be null or empty.");

            _delBit = 0;
            _specPtr = -1;
            _nextProductPtr = -1;
            Name = name;
            Type = type;
            _fileOffset = -1;
        }

        public void SetSpecPtr(int specPtr)
        {
            SpecPtr = specPtr;
        }

        public void SetNextProductPtr(int nextProductPtr)
        {
            NextProductPtr = nextProductPtr;
        }

        public void SetFileOffset(int fileOffset)
        {
            if (fileOffset < -1)
                throw new ArgumentException("File offset must be -1 or greater.");
            FileOffset = fileOffset;
        }

        public void SetComponentName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Component name cannot be null or empty.");
            Name = newName;
        }

        public void MarkAsDeleted() => _delBit = 1;
        public void Restore() => _delBit = 0;
    }
}
