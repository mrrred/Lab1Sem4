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
        private Repository _productRepo;
        private bool _filesOpen;

        public FileService()
        {
            _productRepo = null!;
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

                string fullPath = fileName;
                string specFilePath = specFileName;

                if (File.Exists(fileName))
                {
                    Console.Write("File exists. Overwrite? (y/n): ");
                    if (Console.ReadLine()?.ToLower() != "y")
                        return;
                }

                var productFs = new ProductFSManager(fullPath);
                var productSerializer = new ProductSerializer(dataLength);
                var specFs = new SpecFSManager(specFilePath);
                var specSerializer = new SpecSerializer();

                _productRepo = new Repository(productFs, specFs, productSerializer, specSerializer);
                _productRepo.Create(fullPath, dataLength, specFileName);

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

                var fullPath = fileName;
                string directory = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directory))
                    directory = ".";

                var productFs = new ProductFSManager(fullPath);
                productFs.OpenFile();
                var productHeader = productFs.ReadHeader();

                var productSerializer = new ProductSerializer(productHeader.DataLength);
                string specFileName = productHeader.SpecFileName;
                string specFilePath = Path.Combine(directory, specFileName);

                if (!File.Exists(specFilePath))
                {
                    Console.WriteLine("Error: specification file not found: " + specFilePath);
                    productFs.Close();
                    return;
                }

                var specFs = new SpecFSManager(specFilePath);
                var specSerializer = new SpecSerializer();

                _productRepo = new Repository(productFs, specFs, productSerializer, specSerializer);
                _productRepo.Open(fullPath);

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
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error: NullReferenceException - " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
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

                var spec = new Spec(specComponent.FileOffset, multiplicity);
                _productRepo.AddSpec(component, spec);
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

                _productRepo.DeleteSpec(component, specificationName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Restore(string componentName)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                var product = _productRepo.FindIncludeDeleted(componentName);
                if (product == null)
                {
                    Console.WriteLine("Error: component not found: " + componentName);
                    return;
                }

                _productRepo.Restore(componentName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Restore()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.RestoreAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Truncate()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.Truncate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
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
                    Console.WriteLine("Error: component not found: " + componentName);
                    return;
                }

                if (component.Type == ComponentType.Detail)
                {
                    Console.WriteLine("Error: detail cannot contain components");
                    return;
                }

                PrintComponentTree(component, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Print()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                Console.WriteLine("{0,-20} {1}", "Name", "Type");
                foreach (var product in _productRepo.GetAll().OrderBy(p => p.Name))
                {
                    Console.WriteLine("{0,-20} {1}", product.Name, product.Type);
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

        private void PrintHelp(string fileName)
        {
            var helpText = new[]
            {
                "Create <filename> <length> [spec_file] - Create files",
                "Open <filename> - Open files",
                "Input <name> <type> - Add component (Product, Node, Detail)",
                "Input <component> <component> [multiplicity] - Add to specification",
                "Delete <name> - Delete component",
                "Delete <component> <component> - Delete from specification",
                "Restore <name> - Restore component",
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
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private bool ValidateFilesOpen()
        {
            if (!_filesOpen)
            {
                Console.WriteLine("Error: files not open");
                return false;
            }
            return true;
        }

        private void PrintComponentTree(Product component, string prefix)
        {
            Console.WriteLine(prefix + component.Name);

            if (component.Type == ComponentType.Detail || component.SpecPtr == -1)
                return;

            var allProducts = _productRepo.GetAll().ToList();
            var allSpecs = _productRepo.GetSpecsForProduct(component.FileOffset).ToList();
            var specList = allSpecs.Where(s => !s.IsDeleted).ToList();

            for (int i = 0; i < specList.Count; i++)
            {
                var spec = specList[i];
                var child = allProducts.FirstOrDefault(p => p.FileOffset == spec.ComponentPtr);

                if (child != null)
                {
                    string multiplicity = spec.Multiplicity > 1 ? " (x" + spec.Multiplicity + ")" : "";
                    string connector = i == specList.Count - 1 ? "└─" : "├─";
                    Console.WriteLine(prefix + connector + " " + child.Name + multiplicity);
                    
                    if (child.Type != ComponentType.Detail)
                    {
                        string newPrefix = prefix + (i == specList.Count - 1 ? "  " : "│ ");
                        PrintComponentTree(child, newPrefix);
                    }
                }
            }
        }
    }
}
