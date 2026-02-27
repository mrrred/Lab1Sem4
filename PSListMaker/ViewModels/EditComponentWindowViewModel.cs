using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class EditComponentWindowViewModel
    {
        private readonly IFileService _fileService;


        public EditComponentWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }
    }
}
