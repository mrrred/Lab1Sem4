using ConsoleApp2.MenuService;
using PSListMaker.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.WindowServices
{
    public class ComponentListService : IComponentListService
    {
        private IFileService _fileService;

        public ComponentListService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public AddComponentWindow GetAddWindow()
        {
            return new AddComponentWindow(new ViewModels.AddComponentWindowViewModel(_fileService));
        }

        public EditComponentWindow GetEditWindow()
        {
            return new EditComponentWindow(new ViewModels.EditComponentWindowViewModel(_fileService));
        }
    }
}
