using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class AddSpecificationWindowViewModel
    {
        private IFileService _fileService;

        private readonly string _componentName;

        public AddSpecificationWindowViewModel(string componentName, IFileService fileService)
        {
            _fileService = fileService;
            _componentName = componentName;
        }

        public List<string> GetAllProductNames()
        {
            var allNames = _fileService.GetAllProducts()
                .Where(x => x.Type != ConsoleApp2.Entities.ComponentType.Product)
                .Select(x => x.Name)
                .ToList();

            var existingSpecNames = new HashSet<string>(
                _fileService.GetProductSpecifications(_componentName)
                    .Select(p => p.Name));

            return allNames.Where(n => !existingSpecNames.Contains(n)).ToList();
        }

        public void AddSpecification(string specification, ushort multiplicity = 1)
        {
            _fileService.Input(_componentName, specification, multiplicity);
        }

        public void RegisterOnChange(EventHandler e)
        {
            _fileService.ProductsChanged += e;
        }

        public void UnRegisterOnChange(EventHandler e)
        {
            _fileService.ProductsChanged -= e;
        }
    }
}
