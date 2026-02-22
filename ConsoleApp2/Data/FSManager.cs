using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConsoleApp2.Data.Abstractions;

namespace ConsoleApp2.Data
{
    public class FSManager : IFSManager
    {
        private string _filePath;
        private FileStream _fileStream;
        private bool _isProductFile;

        public FSManager(string filePath, bool isProductFile)
        {
            _filePath = filePath;
            _isProductFile = isProductFile;
        }

        public FileStream GetStream() => _fileStream;

        public void WriteHeader(FileHeader header)
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

                if (_isProductFile && !string.IsNullOrEmpty(header.SpecFName))
                {
                    char[] specBuffer = new char[16];
                    Array.Copy(header.SpecFName.ToCharArray(), specBuffer, 
                        Math.Min(header.SpecFName.Length, 16));
                    writer.Write(specBuffer);
                }
            }
            _fileStream.Flush();
        }

        public FileHeader ReadHeader()
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

                var header = new FileHeader
                {
                    Signature = signature,
                    DataLength = reader.ReadInt16(),
                    FirstRecPtr = reader.ReadInt32(),
                    UnclaimedPtr = reader.ReadInt32()
                };

                if (_isProductFile)
                {
                    char[] specName = reader.ReadChars(16);
                    header.SpecFName = new string(specName).TrimEnd();
                }

                return header;
            }
        }

        public void Seek(long offset)
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File not opened");
            _fileStream.Seek(offset, SeekOrigin.Begin);
        }

        public long GetPosition()
        {
            return _fileStream?.Position ?? -1;
        }

        public void Close()
        {
            _fileStream?.Close();
            _fileStream?.Dispose();
        }

        public void CreateFile()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            _fileStream = new FileStream(_filePath, FileMode.CreateNew, FileAccess.ReadWrite);
        }

        public void OpenFile()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File {_filePath} not found");

            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);
        }
    }
}
