using ConsoleApp2.MenuService;
using PSListMaker.Commands;
using PSListMaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PSListMaker.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly IFileServiceWithActionHistory _fileService;


        public MainWindowViewModel(IFileServiceWithActionHistory fileService)
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

        public void DeleteTempFiles()
        {
            _fileService.Close();
            _fileService.DeleteTempFiles();
        }

        public void RegisterOnError(EventHandler<string> handler)
        {
            _fileService.ErrorOccurred += handler;
        }

        public void UnRegisterOnError(EventHandler<string> handler)
        {
            _fileService.ErrorOccurred -= handler;
        }
    }
}
