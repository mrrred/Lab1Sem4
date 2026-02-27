using ConsoleApp2.MenuService;
using PSListMaker.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace PSListMaker.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly IFileService _fileService;


        public MainWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void CreateFile(string path)
        {
            string Directory = Path.GetDirectoryName(path);
            string fileNames = Path.GetFileNameWithoutExtension(path);

            // Пока так
            _fileService.Create(Directory, fileNames, fileNames, 20);
        }

        public void OpenFile(string path)
        {
            _fileService.Open(path);
        }
    }
}
