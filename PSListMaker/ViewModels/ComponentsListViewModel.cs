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
        private IFileServiceWithActionHistory _fileService;

        public bool IsUnsave => _fileService.IsUnsave;

        public bool IsCanUndo => _fileService.IsCanUndo;

        public ComponentsListViewModel(IFileServiceWithActionHistory fileService)
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
            _fileService.Delete(name);
        }

        public void Save()
        {
            _fileService.Truncate();
            _fileService.Save();
        }

        public void Undo()
        {
            _fileService.Undo();
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
