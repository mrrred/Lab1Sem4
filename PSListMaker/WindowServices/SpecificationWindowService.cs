using ConsoleApp2.MenuService;
using PSListMaker.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.WindowServices
{
    public class SpecificationWindowService : ISpecificationWindowService
    {
        private readonly IFileService _fileService;

        public SpecificationWindowService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public AddSpecificationWindow GetAddWindow(string componentName)
        {
            return new AddSpecificationWindow(componentName, 
                new ViewModels.AddSpecificationWindowViewModel(componentName, _fileService));
        }
    }
}
