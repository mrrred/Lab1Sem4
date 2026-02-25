using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;

namespace ConsoleApp2.Data
{
    public class SpecFSManager : BaseFSManager
    {
        public SpecFSManager(string filePath) : base(filePath)
        {
        }

        public void WriteHeader(SpecHeader header)
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File not opened.");

            using (var writer = new BinaryWriter(_fileStream, Encoding.UTF8, leaveOpen: true))
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
                writer.Write(header.FirstRecPtr);
                writer.Write(header.UnclaimedPtr);
            }
            _fileStream.Flush();
        }

        public SpecHeader ReadHeader()
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File not opened.");

            using (var reader = new BinaryReader(_fileStream, Encoding.UTF8, leaveOpen: true))
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
                int firstRecPtr = reader.ReadInt32();
                int unclaimedPtr = reader.ReadInt32();
                var header = new SpecHeader(firstRecPtr, unclaimedPtr);
                return header;
            }
        }
    }
}
