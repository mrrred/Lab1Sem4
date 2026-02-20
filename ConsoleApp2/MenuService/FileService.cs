using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.MenuService
{
    internal class FileService : IFileService
    {
        private IProductManaging _productRepo;
        private ISpecManaging _specRepo;
        private string _currentProductPath;
        private string _currentSpecPath;
        private bool _filesOpen;

        public FileService(IProductManaging productRepo, ISpecManaging specRepo)
        {
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _specRepo = specRepo ?? throw new ArgumentNullException(nameof(specRepo));
            _filesOpen = false;
        }

        public void Create(string arguments)
        {
            try
            {
                var args = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (args.Length < 2)
                {
                    Console.WriteLine("Error: not enough arguments");
                    Console.WriteLine("Usage: Create <filename> <max_length> [spec_filename]");
                    return;
                }

                string fileName = args[0];
                if (!short.TryParse(args[1], out short dataLength))
                {
                    Console.WriteLine("Error: invalid data length");
                    return;
                }

                string specFileName = args.Length > 2 ? args[2] :
                    Path.GetFileNameWithoutExtension(fileName) + ".prs";

                if (!fileName.EndsWith(".prd"))
                    fileName += ".prd";
                if (!specFileName.EndsWith(".prs"))
                    specFileName += ".prs";

                if (File.Exists(fileName))
                {
                    Console.Write("File exists. Overwrite? (y/n): ");
                    if (Console.ReadLine()?.ToLower() != "y")
                        return;
                }

                _productRepo.Create(fileName, dataLength, Path.GetFileName(specFileName));
                _specRepo.Create(specFileName);
                _currentProductPath = fileName;
                _currentSpecPath = specFileName;
                _filesOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Open(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    Console.WriteLine("Error: specify filename");
                    return;
                }

                if (!fileName.EndsWith(".prd"))
                    fileName += ".prd";

                if (!File.Exists(fileName))
                {
                    Console.WriteLine("Error: file not found: " + fileName);
                    return;
                }

                _productRepo.Open(fileName);

                string specFileName = Path.GetFileNameWithoutExtension(fileName) + ".prs";
                _specRepo.Open(specFileName);

                _currentProductPath = fileName;
                _currentSpecPath = specFileName;
                _filesOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Input(string componentName, ComponentType type)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                if (string.IsNullOrWhiteSpace(componentName))
                {
                    Console.WriteLine("Error: specify component name");
                    return;
                }

                var product = new Product(componentName, type);
                _productRepo.Add(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Input(string componentName, string specificationName)
        {
            Input(componentName, specificationName, 1);
        }

        public void Input(string componentName, string specificationName, ushort multiplicity)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var component = _productRepo.Find(componentName);
                if (component == null)
                {
                    Console.WriteLine("Error: component not found: " + componentName);
                    return;
                }

                if (component.Type == ComponentType.Detail)
                {
                    Console.WriteLine("Error: detail cannot contain components");
                    return;
                }

                var specComponent = _productRepo.Find(specificationName);
                if (specComponent == null)
                {
                    Console.WriteLine("Error: component not found: " + specificationName);
                    return;
                }

                if (multiplicity <= 0)
                {
                    Console.WriteLine("Error: multiplicity must be greater than 0");
                    return;
                }

                var specification = new Spec(specComponent.FileOffset, multiplicity);
                _specRepo.Add(specification);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Delete(string componentName)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var product = _productRepo.Find(componentName);
                if (product == null)
                {
                    Console.WriteLine("Error: component not found: " + componentName);
                    return;
                }

                var referencesInSpecs = _specRepo.GetByComponentPtr(product.FileOffset).Where(s => !s.IsDeleted).ToList();
                
                if (referencesInSpecs.Count > 0)
                {
                    Console.WriteLine("Error: cannot delete component, it is referenced in specifications");
                    return;
                }

                _productRepo.Delete(componentName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Delete(string componentName, string specificationName)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var component = _productRepo.Find(componentName);
                if (component == null)
                {
                    Console.WriteLine("component not found: " + componentName);
                    return;
                }

                if (component.Type == ComponentType.Detail)
                {
                    Console.WriteLine("detail cannot contain components");
                    return;
                }

                var specComponent = _productRepo.Find(specificationName);
                if (specComponent == null)
                {
                    Console.WriteLine("component not found: " + specificationName);
                    return;
                }

                var specifications = _specRepo.GetByProductOffset(component.FileOffset);
                var specToDelete = specifications.FirstOrDefault(s => s.ComponentPtr == specComponent.FileOffset && !s.IsDeleted);

                if (specToDelete == null)
                {
                    Console.WriteLine("specification not found");
                    return;
                }

                _specRepo.Delete(specToDelete.FileOffset);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Restore(string componentName)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var product = _productRepo.Find(componentName);
                if (product == null)
                {
                    Console.WriteLine("component not found: " + componentName);
                    return;
                }

                _productRepo.Restore(componentName);
                
                var allSpecs = _specRepo.GetByProductOffset(product.FileOffset).ToList();
                foreach (var spec in allSpecs)
                {
                    if (spec.IsDeleted)
                    {
                        _specRepo.Restore(spec.FileOffset);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Restore()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.RestoreAll();
                _specRepo.RestoreAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Truncate()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.Truncate();
                _specRepo.Truncate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Print(string componentName)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var component = _productRepo.Find(componentName);
                if (component == null)
                {
                    Console.WriteLine("component not found: " + componentName);
                    return;
                }

                if (component.Type == ComponentType.Detail)
                {
                    Console.WriteLine("detail cannot contain components");
                    return;
                }

                PrintComponentTree(component, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Print()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                Console.WriteLine("Name Type");
                foreach (var product in _productRepo.GetAll().OrderBy(p => p.Name))
                {
                    Console.WriteLine(product.Name + " " + product.Type);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Help()
        {
            PrintHelp(null);
        }

        public void Help(string fileName)
        {
            PrintHelp(fileName);
        }

        private void PrintHelp(string? fileName)
        {
            var helpText = new[]
            {
                "Create <filename> <length> [spec_file] - Create files",
                "Open <filename> - Open files",
                "Input <name> <type> - Add component (Product, Node, Detail)",
                "Input <component> <component> [multiplicity] - Add to specification",
                "Delete <name> - Delete component (if no references)",
                "Delete <component> <component> - Delete from specification",
                "Restore <name> - Restore component and its specifications",
                "Restore (*) - Restore all",
                "Truncate - Physical deletion",
                "Print <name> - Show specification",
                "Print (*) - Show all components",
                "Help [file] - Help",
                "Exit - Exit"
            };

            if (string.IsNullOrWhiteSpace(fileName))
            {
                foreach (var line in helpText)
                    Console.WriteLine(line);
            }
            else
            {
                try
                {
                    File.WriteAllLines(fileName, helpText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool ValidateFilesOpen()
        {
            if (!_filesOpen)
            {
                Console.WriteLine("files not open");
                return false;
            }
            return true;
        }

        private void PrintComponentTree(Product component, string prefix)
        {
            Console.WriteLine(prefix + component.Name + " (" + component.Type + ")");

            var specifications = _specRepo.GetByProductOffset(component.FileOffset);
            var specList = specifications.Where(s => !s.IsDeleted).ToList();

            for (int i = 0; i < specList.Count; i++)
            {
                var spec = specList[i];
                var child = _productRepo.GetAll().FirstOrDefault(p => p.FileOffset == spec.ComponentPtr);

                if (child != null)
                {
                    string multiplicity = spec.Multiplicity > 1 ? " (x" + spec.Multiplicity + ")" : "";
                    string newPrefix = prefix + (i == specList.Count - 1 ? "  " : "  ");
                    Console.WriteLine(newPrefix + child.Name + " (" + child.Type + ")" + multiplicity);
                }
            }
        }
    }
}
