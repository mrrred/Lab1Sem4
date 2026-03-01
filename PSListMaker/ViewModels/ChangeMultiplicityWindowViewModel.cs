using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class ChangeMultiplicityWindowViewModel
    {
        private IFileService _fileService;

        private string _component;

        private string _specs;

        public ChangeMultiplicityWindowViewModel(string component, string specs, IFileService fileService)
        {
            _fileService = fileService;
            _component = component;
            _specs = specs;
        }

        public void ChangeMultiplicity(ushort newMultiplicity)
        {
            _fileService.EditSpec(_component, _specs, newMultiplicity);
        }


    }
}
