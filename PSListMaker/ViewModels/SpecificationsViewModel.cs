using ConsoleApp2.Entities;
using ConsoleApp2.MenuService;
using PSListMaker.Models;
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

        public List<ComponentsWithSpecs> Components => GetComponentsTree();

        public List<ComponentsWithSpecs> GetComponentsTree()
        {
            var componentsWithSpecs = new List<ComponentsWithSpecs>();

            var components = _fileService.GetAllProducts();

            foreach (var component in components)
            {
                if (component.Type == ComponentType.Product)
                {
                    var componentsWithSpec = new ComponentsWithSpecs(component.Name, 
                        component.Type.ToString());

                    AddSpecifications(componentsWithSpec);

                    componentsWithSpecs.Add(componentsWithSpec);
                }
            }

            return componentsWithSpecs;
        }

        private void AddSpecifications(ComponentsWithSpecs component)
        {
            var specs = _fileService.GetProductSpecifications(component.Name);

            if (specs != null)
            {
                foreach (var spec in specs)
                {
                    var child = new ComponentsWithSpecs(spec.Name, 
                        _fileService.GetProduct(spec.Name).Type.ToString());

                    AddSpecifications(child);

                    component.Specs.Add(child);
                }
            }
        }

        public void AddSpecs(string componentName, string specificationName, ushort multiplicity)
        {
            _fileService.Input(componentName, specificationName, multiplicity);
        }

        public void RemoveSpecs(string componentName, string specificationName)
        {
            _fileService.Delete(componentName, specificationName);
        }
    }
}
