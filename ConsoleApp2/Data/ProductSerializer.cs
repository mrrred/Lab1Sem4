using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.IO;
using System.Text;

namespace ConsoleApp2.Data
{

    public class ProductSerializer : ISerializer<Product>
    {
        private short _dataLength;

        public ProductSerializer(short dataLength)
        {
            if (dataLength <= 0)
                throw new ArgumentException("Длина данных должна быть положительной", nameof(dataLength));
            _dataLength = dataLength;
        }

        public void WriteToFile(Product entity, BinaryWriter writer)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            writer.Write(entity.DelBit);            
            writer.Write(entity.SpecPtr);             
            writer.Write(entity.NextProductPtr);      

            byte[] nameBytes = Encoding.UTF8.GetBytes(entity.Name ?? string.Empty);
            byte[] buffer = new byte[_dataLength];
            Array.Copy(nameBytes, buffer, Math.Min(nameBytes.Length, _dataLength));
            writer.Write(buffer);                     
        }

        public Product ReadFromFile(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            byte delBit = reader.ReadByte();
            int specPtr = reader.ReadInt32();
            int nextPtr = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(_dataLength);
            string name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0', ' ');

            return new Product(name, ComponentType.Product)
            {
                DelBit = delBit,
                SpecPtr = specPtr,
                NextProductPtr = nextPtr
            };
        }

        public int GetEntitySize() => FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE + _dataLength; 
    }
}
