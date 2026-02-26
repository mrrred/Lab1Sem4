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
                throw new ArgumentException("Data length must be greater than zero.", nameof(dataLength));
            _dataLength = dataLength;
        }

        public void WriteToFile(Product product, BinaryWriter writer)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product), "Product is null.");
            
            writer.Write(product.DelBit);
            writer.Write((byte)product.Type);
            writer.Write(product.SpecPtr);
            writer.Write(product.NextProductPtr);
            
            byte[] nameBytes = Encoding.UTF8.GetBytes(product.Name ?? string.Empty);
            byte[] buffer = new byte[_dataLength];
            Array.Copy(nameBytes, buffer, Math.Min(nameBytes.Length, _dataLength));
            writer.Write(buffer);
        }

        public Product ReadFromFile(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "Reader is null.");
            
            byte delBit = reader.ReadByte();
            ComponentType type = (ComponentType)reader.ReadByte();
            int specPtr = reader.ReadInt32();
            int nextPtr = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(_dataLength);
            string name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0', ' ');
            
            Product product = new Product(name, type)
            {
                DelBit = delBit,
                SpecPtr = specPtr,
                NextProductPtr = nextPtr
            };
            
            return product;
        }

        public int GetEntitySize() => FileStructure.DELBIT_SIZE + FileStructure.TYPE_SIZE + FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE + _dataLength;
    }
}
