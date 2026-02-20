using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Entities
{
    public class Spec : IEntity
    {
        public byte DelBit { get; set; } // 1B
        public int ComponentPtr { get; set; } // 4B
        public ushort Multiplicity { get; set; } // 2B
        public int NextSpecPtr { get; set; } // 4B
        public int FileOffset { get; set; } // 4B
        public string Name => $"Spec_{FileOffset}";
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
