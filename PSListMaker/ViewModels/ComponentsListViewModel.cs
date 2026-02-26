using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class ComponentsListViewModel
    {
        private IFileService _fileService;

        public ComponentsListViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }
    }
}
