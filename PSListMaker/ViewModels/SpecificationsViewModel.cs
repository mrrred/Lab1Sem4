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

        public List<ComponentsWithSpecs> GetComponentsTree()
        {
            var componentsWithSpecs = new List<ComponentsWithSpecs>();

            var components = _fileService.GetAllProducts();

            foreach (var component in components)
            {
                if (component.Type == ComponentType.Product)
                {
                    var componentsWithSpec = new ComponentsWithSpecs(component.Name);

                    AddSpecifications(componentsWithSpec);

                    componentsWithSpecs.Add(componentsWithSpec);
                }
            }

            return componentsWithSpecs;
        }

        private void AddSpecifications(ComponentsWithSpecs component)
        {
            var componentsWithSpecs = new List<ComponentsWithSpecs>();

            var specs = _fileService.GetProductSpecifications(component.Name);

            if (specs != null)
            {
                foreach (var spec in specs)
                {
                    var componentsWithSpec = new ComponentsWithSpecs(spec.Name);

                    AddSpecifications(componentsWithSpec);

                    componentsWithSpecs.Add(componentsWithSpec);
                }
            }
        }
    }
}
