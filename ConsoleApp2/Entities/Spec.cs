using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Entities
{
    public class Spec : IEntity
    {
        private byte _delbit;
        private int _componentPtr;
        private ushort _multiplicity;
        private int _nextSpecPtr;
        private int _fileOffset;
        private string _name;

        public byte DelBit
        {
            get { return _delbit; }
            set { _delbit = value; }
        }

        public int ComponentPtr
        {
            get { return _componentPtr; }
            private set { _componentPtr = value; }
        } 

        public ushort Multiplicity
        {
            get { return _multiplicity; }
            private set { _multiplicity = value; }
        }

        public int NextSpecPtr
        {
            get { return _nextSpecPtr; }
            set { _nextSpecPtr = value; }
        }

        public int FileOffset
        {
            get { return _fileOffset; }
            set { _fileOffset = value; }
        }

        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public bool IsDeleted => DelBit == 1;

        public Spec(int componentPtr, ushort multiplicity = 1)
        {
            _delbit = 0;
            SetComponentPtr(componentPtr);
            SetMultiplicity(multiplicity);
            _nextSpecPtr = -1;
            _fileOffset = -1;
            _name = null;
        }

        public void SetComponentPtr(int componentPtr)
        {
            if (componentPtr < 0)
                throw new ArgumentException("Component pointer must be non-negative.");
            ComponentPtr = componentPtr;
        }

        public void SetMultiplicity(ushort multiplicity)
        {
            if (multiplicity <= 0)
                throw new ArgumentException("Multiplicity must be greater than zero.");
            Multiplicity = multiplicity;
        }

        public void SetNextSpecPtr(int nextSpecPtr)
        {
            NextSpecPtr = nextSpecPtr;
        }

        public void SetFileOffset(int fileOffset)
        {
            if (fileOffset < -1)
                throw new ArgumentException("File offset must be -1 or greater.");
            FileOffset = fileOffset;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void MarkAsDeleted() => _delbit = 1;
        public void Restore() => _delbit = 0;
    }
}

