using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using PSListMaker.Models;
using System.Windows.Input;

namespace PSListMaker.ViewModels
{
    public class ComponentsListViewModel
    {
        private IFileService _fileService;

        public ICommand Undo { get; set; }
        public ICommand Save { get; set; }

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

        public void DeleteComponent(string name)
        {
            // With Truncate
            _fileService.Delete(name);
            _fileService.Truncate();
        }

        public void RegisterOnChange(EventHandler update)
        {
            _fileService.ProductsChanged += update;
        }

        public void UnRegisterOnChange(EventHandler update)
        {
            _fileService.ProductsChanged -= update;
        }
    }
}
