using ConsoleApp2.Data;
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
        private short _currentDataLength;

        public FileService()
        {
            _productRepo = null;
            _specRepo = null;
            _currentProductPath = null;
            _currentSpecPath = null;
            _filesOpen = false;
            _currentDataLength = 50;
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

                if (dataLength <= 0 || dataLength > 32000)
                {
                    Console.WriteLine("Error: data length must be between 1 and 32000");
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

                InitializeRepositories(fileName, specFileName, dataLength);
                
                _productRepo.Create(fileName, dataLength, Path.GetFileName(specFileName));
                _specRepo.Create(specFileName);
                _currentProductPath = fileName;
                _currentSpecPath = specFileName;
                _currentDataLength = dataLength;
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

                string specFileName = Path.GetFileNameWithoutExtension(fileName) + ".prs";
                
                var tempFsManager = new FSManager(fileName, isProductFile: true);
                tempFsManager.OpenFile();
                var header = tempFsManager.ReadHeader();
                tempFsManager.Close();
                
                _currentDataLength = header.DataLength;
                
                InitializeRepositories(fileName, specFileName, _currentDataLength);
                
                _productRepo.Open(fileName);
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

        private void InitializeRepositories(string productFileName, string specFileName, short dataLength)
        {
            var productFsManager = new FSManager(productFileName, isProductFile: true);
            var productSerializer = new ProductSerializer(dataLength: dataLength);
            var productHeader = new FileHeader();
            var productNodeNavigator = new ProductNodeNavigator(productHeader);
            var productListManager = new LLManager<Product>(
                productFsManager,
                productSerializer,
                productNodeNavigator
            );
            _productRepo = new ProductRepository(
                productFsManager,
                productSerializer,
                productListManager,
                productNodeNavigator
            );

            var specFsManager = new FSManager(specFileName, isProductFile: false);
            var specSerializer = new SpecSerializer();
            var specHeader = new FileHeader();
            var specNodeNavigator = new SpecNodeNavigator(specHeader);
            var specListManager = new LLManager<Spec>(
                specFsManager,
                specSerializer,
                specNodeNavigator
            );
            _specRepo = new SpecRepository(
                specFsManager,
                specSerializer,
                specListManager,
                specNodeNavigator
            );
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
                int specOffset = _specRepo.AddAndGetOffset(specification);
                
                // Link specification to component
                if (component.SpecPtr == -1)
                {
                    // First specification for this component
                    component.SpecPtr = specOffset;
                    _productRepo.Update(component);
                }
                else
                {
                    // Find last specification and link it to the new one
                    var allSpecs = _specRepo.GetAll();
                    int currentSpecPtr = component.SpecPtr;
                    Spec lastSpec = null;
                    
                    while (currentSpecPtr != -1)
                    {
                        var spec = allSpecs.FirstOrDefault(s => s.FileOffset == currentSpecPtr);
                        if (spec == null || spec.IsDeleted)
                            break;
                        
                        lastSpec = spec;
                        currentSpecPtr = spec.NextSpecPtr;
                    }
                    
                    if (lastSpec != null)
                    {
                        lastSpec.NextSpecPtr = specOffset;
                        _specRepo.Update(lastSpec);
                    }
                }
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

                var allSpecs = _specRepo.GetAll();
                int currentSpecPtr = component.SpecPtr;
                Spec specToDelete = null;
                Spec prevSpec = null;

                while (currentSpecPtr != -1)
                {
                    var spec = allSpecs.FirstOrDefault(s => s.FileOffset == currentSpecPtr);
                    if (spec == null || spec.IsDeleted)
                        break;

                    if (spec.ComponentPtr == specComponent.FileOffset)
                    {
                        specToDelete = spec;
                        break;
                    }

                    prevSpec = spec;
                    currentSpecPtr = spec.NextSpecPtr;
                }

                if (specToDelete == null)
                {
                    Console.WriteLine("specification not found");
                    return;
                }

                _specRepo.Delete(specToDelete.FileOffset);

                if (prevSpec != null)
                {
                    prevSpec.NextSpecPtr = specToDelete.NextSpecPtr;
                    _specRepo.Update(prevSpec);
                }
                else if (component.SpecPtr == specToDelete.FileOffset)
                {
                    component.SpecPtr = specToDelete.NextSpecPtr;
                    _productRepo.Update(component);
                }
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
                
                var allSpecs = _specRepo.GetAll();
                int currentSpecPtr = product.SpecPtr;
                
                while (currentSpecPtr != -1)
                {
                    var spec = allSpecs.FirstOrDefault(s => s.FileOffset == currentSpecPtr);
                    if (spec == null)
                        break;
                    
                    if (spec.IsDeleted)
                    {
                        _specRepo.Restore(spec.FileOffset);
                    }
                    
                    currentSpecPtr = spec.NextSpecPtr;
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

            if (component.Type == ComponentType.Detail || component.SpecPtr == -1)
                return;

            var allSpecs = _specRepo.GetAll();
            var specList = new List<Spec>();
            
            int currentSpecPtr = component.SpecPtr;
            while (currentSpecPtr != -1)
            {
                var spec = allSpecs.FirstOrDefault(s => s.FileOffset == currentSpecPtr);
                if (spec == null || spec.IsDeleted)
                    break;
                
                specList.Add(spec);
                currentSpecPtr = spec.NextSpecPtr;
            }

            for (int i = 0; i < specList.Count; i++)
            {
                var spec = specList[i];
                var child = _productRepo.GetAll().FirstOrDefault(p => p.FileOffset == spec.ComponentPtr);

                if (child != null)
                {
                    string multiplicity = spec.Multiplicity > 1 ? " (x" + spec.Multiplicity + ")" : "";
                    string connector = i == specList.Count - 1 ? "└─ " : "├─ ";
                    Console.WriteLine(prefix + connector + child.Name + " (" + child.Type + ")" + multiplicity);
                    
                    if (child.Type != ComponentType.Detail)
                    {
                        string newPrefix = prefix + (i == specList.Count - 1 ? "   " : "│  ");
                        PrintComponentTree(child, newPrefix);
                    }
                }
            }
        }
    }
}
