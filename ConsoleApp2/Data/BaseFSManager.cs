using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data
{
    public class BaseFSManager
    {
        protected string _filePath;
        protected FileStream _fileStream;

        public BaseFSManager(string filePath)
        {
            _filePath = filePath;
        }

        public FileStream GetStream() => _fileStream;

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
