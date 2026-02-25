using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.IO;

namespace ConsoleApp2.Data
{
    public class SpecSerializer : ISerializer<Spec>
    {
        public void WriteToFile(Spec entity, BinaryWriter writer)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Spec is null.");

            writer.Write(entity.DelBit);
            writer.Write(entity.ComponentPtr);
            writer.Write(entity.Multiplicity);
            writer.Write(entity.NextSpecPtr);
        }

        public Spec ReadFromFile(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "Reader is null.");

            byte delBit = reader.ReadByte();
            int componentPtr = reader.ReadInt32();
            ushort multiplicity = reader.ReadUInt16();
            int nextPtr = reader.ReadInt32();

            Spec spec = new Spec(componentPtr, multiplicity)
            {
                DelBit = delBit,
                NextSpecPtr = nextPtr
            };
            
            return spec;
        }

        public int GetEntitySize() => 1 + 4 + 2 + 4;
    }
}
