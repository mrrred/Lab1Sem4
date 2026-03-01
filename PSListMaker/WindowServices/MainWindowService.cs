using ConsoleApp2.MenuService;
using PSListMaker.Models;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace PSListMaker.WindowServices
{
    public class MainWindowService : IMainWindowService
    {
        private IFileServiceWithActionHistory _fileService;

        public MainWindowService(IFileServiceWithActionHistory fileService)
        {
            _fileService = fileService;
        }

        public ComponentsList GetComponentsListWindow()
        {
            return new ComponentsList(new ComponentListService(_fileService), new ViewModels.ComponentsListViewModel(_fileService));
        }

        public Specifications GetSpecificationsWindow()
        {
            return new Specifications(new SpecificationWindowService(_fileService), new ViewModels.SpecificationsViewModel(_fileService));
        }
    }
}
