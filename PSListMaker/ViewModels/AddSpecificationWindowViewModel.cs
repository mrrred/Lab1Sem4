using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.ViewModels
{
    public class AddSpecificationWindowViewModel
    {
        private IFileService _fileService;
        public AddSpecificationWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public List<string> GetAllProductNames()
        {
            return _fileService.GetAllProducts()
                .Where(x => x.Type != ConsoleApp2.Entities.ComponentType.Product)
                .Select(x => x.Name)
                .ToList();
        }

        public void AddSpecification(string productName, string specification, ushort multiplicity = 1)
        {
            _fileService.Input(productName, specification, multiplicity);
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
