using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace ConsoleApp2.MenuService
{
    public class TempSaver
    {
        private string _path;

        public string? TempPath { get; private set; }

        private string? _specTempPath;

        public TempSaver(string pathOriginal)
        {
            if (!File.Exists(pathOriginal))
            {
                throw new FileNotFoundException("File is not found.", pathOriginal);
            }

            _path = pathOriginal;
        }

        public void SaveToOrigin()
        {
            if (TempPath == null || TempPath == string.Empty)
            {
                throw new InvalidOperationException("Temp file does not exist.");
            }

            File.Copy(TempPath, _path, true);
        }

        public void CreateTempFile()
        {
            TempPath = Path.Combine(Path.GetDirectoryName(_path) ?? Path.GetTempPath(), 
                $"{Path.GetFileNameWithoutExtension(_path)}.temp.prd");
            File.Create(TempPath).Close();
            File.Copy(_path, TempPath, true);

            _specTempPath = Path.Combine(Path.GetDirectoryName(_path) ?? Path.GetTempPath(),
                $"{Path.GetFileNameWithoutExtension(_path)}.temp.prs");
            File.Create(_specTempPath).Close();
            File.Copy(Path.Combine(Path.GetDirectoryName(_path) ?? Path.GetTempPath(),
                $"{Path.GetFileNameWithoutExtension(_path)}.prs"), _specTempPath, true);
        }

        public void DeleteTempFile()
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }

            if (File.Exists(_specTempPath))
            {
                File.Delete(_specTempPath);
            }

            TempPath = null;
            _specTempPath = null;
        }

        ~TempSaver()
        {
            if (TempPath != null)
            {
                DeleteTempFile();
            }
        }
    }
}
