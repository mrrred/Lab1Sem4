using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using PSListMaker.Models;

namespace PSListMaker.ViewModels
{
    public class ComponentsListViewModel
    {
        private IFileService _fileService;

        public ComponentsListViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public List<ComponentMin> GetComponents()
        {
            // Пока так

            List<ComponentMin> components = new List<ComponentMin>();

            foreach(var comp in _fileService.GetAllProducts())
            {
                components.Add(new ComponentMin(comp.Name, comp.Type.ToString()));
            }

            return components;
        }

        public void RegisterOnChange(EventHandler update)
        {
            _fileService.ProductsChanged += update;
        }
    }
}
