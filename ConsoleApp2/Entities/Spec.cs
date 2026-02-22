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
            set { _componentPtr = value; }
        } 
        public ushort Multiplicity
        {
            get { return _multiplicity; }
            set { _multiplicity = value; }
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
            set { _name = value; }
        }
        public bool IsDeleted => DelBit == 1;

        public Spec(int componentPtr, ushort multiplicity = 1)
        {
            DelBit = 0;
            ComponentPtr = componentPtr;
            Multiplicity = multiplicity;
            NextSpecPtr = -1;
            FileOffset = -1;
        }

        public void MarkAsDeleted() => DelBit = 1;
        public void Restore() => DelBit = 0;
    }
}
