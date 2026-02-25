using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
                throw new InvalidOperationException("File not opened.");
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
            _fileStream = null;
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
                throw new FileNotFoundException($"File not found at {_filePath}.");

            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);
        }

        public FileStream CreateTempFile()
        {
            string tempFilePath = _filePath + ".tmp";

            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            return new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.ReadWrite);
        }

        public string GetTempFilePath()
        {
            return _filePath + ".tmp";
        }

        public void ReplaceWithTempFile()
        {
            string tempFilePath = GetTempFilePath();

            if (!File.Exists(tempFilePath))
                throw new FileNotFoundException($"Temp file not found at {tempFilePath}.");

            Close();

            if (File.Exists(_filePath))
                File.Delete(_filePath);

            File.Move(tempFilePath, _filePath);

            OpenFile();
        }
    }
}
