using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class SpecificationsViewModel
    {
        private IFileService _fileService;

        public SpecificationsViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }
    }
}
