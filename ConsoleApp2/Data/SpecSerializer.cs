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
                throw new ArgumentNullException(nameof(entity));

            writer.Write(entity.DelBit);            
            writer.Write(entity.ComponentPtr);        
            writer.Write(entity.Multiplicity);        
            writer.Write(entity.NextSpecPtr);        
        }

        public Spec ReadFromFile(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            byte delBit = reader.ReadByte();
            int componentPtr = reader.ReadInt32();
            ushort multiplicity = reader.ReadUInt16();
            int nextPtr = reader.ReadInt32();

            return new Spec(componentPtr, multiplicity)
            {
                DelBit = delBit,
                NextSpecPtr = nextPtr
            };
        }

        public int GetEntitySize() => 1 + FileStructure.POINTER_SIZE + 2 + FileStructure.POINTER_SIZE; 
    }
}
