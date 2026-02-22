using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConsoleApp2.Data.Abstractions;

namespace ConsoleApp2.Data
{
    public class ProductFSManager : BaseFSManager
    {
        public ProductFSManager(string filePath) : base(filePath)
        {
        }

        public void WriteHeader(ProductHeader header)
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File not opened");

            using (var writer = new BinaryWriter(_fileStream, Encoding.UTF8, leaveOpen: true))
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
                writer.Write(Encoding.ASCII.GetBytes(header.Signature.PadRight(2).Substring(0, 2)));
                writer.Write(header.DataLength);
                writer.Write(header.FirstRecPtr);
                writer.Write(header.UnclaimedPtr);

                if (!string.IsNullOrEmpty(header.SpecFileName))
                {
                    char[] specBuffer = new char[16];
                    Array.Copy(header.SpecFileName.ToCharArray(), specBuffer, 
                        Math.Min(header.SpecFileName.Length, 16));
                    writer.Write(specBuffer);
                }
            }
            _fileStream.Flush();
        }

        public ProductHeader ReadHeader()
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File not opened");

            using (var reader = new BinaryReader(_fileStream, Encoding.UTF8, leaveOpen: true))
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
                byte[] sig = reader.ReadBytes(2);
                string signature = Encoding.ASCII.GetString(sig);

                if (signature != "PS")
                    throw new InvalidOperationException("Invalid file signature");

                short dataLength = reader.ReadInt16();
                int firstRecPtr = reader.ReadInt32();
                int unclaimedPtr = reader.ReadInt32();
                char[] specName = reader.ReadChars(16);
                
                string specFileName = new string(specName).TrimEnd('\0');
                
                var header = new ProductHeader(signature, dataLength, firstRecPtr, unclaimedPtr, specFileName);

                return header;
            }
        }

        public ProductHeader ReadProductHeader()
        {
            return ReadHeader();
        }
    }
}
