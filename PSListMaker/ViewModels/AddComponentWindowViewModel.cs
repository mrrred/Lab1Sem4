using ConsoleApp2.Entities;
using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class AddComponentWindowViewModel
    {
        // Пока что так
        private IFileService _fileService;

        public AddComponentWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void Add(string name, ComponentType type)
        {
            _fileService.Input(name, type);
        }
    }
}
