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
        public event EventHandler ProductsChanged;
        public event EventHandler<string> ErrorOccurred;
        public bool IsFilesOpen => _filesOpen;

        public FileService()
        {
            _productRepo = null!;
            _filesOpen = false;
        }

        public void Create(string arguments)
        {
            try
            {
                Close();

                var args = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (args.Length < 2)
                    throw new ArgumentException("Not enough arguments.");

                string fileName = args[0];
                if (!short.TryParse(args[1], out short dataLength))
                    throw new ArgumentException("Invalid data length.");

                if (dataLength <= 0 || dataLength > 32000)
                    throw new ArgumentException("Data length must be between 1 and 32000.");

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
                    Console.Write("File exists. Overwrite. (y/n): ");
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
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Open(string fileName)
        {
            try
            {
                Close();

                if (!fileName.EndsWith(".prd"))
                    fileName += ".prd";

                if (!File.Exists(fileName))
                    throw new FileNotFoundException($"File not found.");

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
                    throw new FileNotFoundException($"Spec file not found.");

                var specFs = new SpecFSManager(specFilePath);
                var specSerializer = new SpecSerializer();

                _productRepo = new Repository(productFs, specFs, productSerializer, specSerializer);
                _productRepo.Open(fullPath);

                _filesOpen = true;
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Close()
        {
            try
            {
                if (_filesOpen && _productRepo != null)
                {
                    _productRepo.Close();
                    _filesOpen = false;
                    _productRepo = null;
                }
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Input(string componentName, ComponentType type)
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                if (string.IsNullOrWhiteSpace(componentName))
                    throw new ArgumentException("Specify component name.");

                var product = new Product(componentName, type);
                _productRepo.Add(product);
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                    throw new InvalidOperationException("Component not found.");

                if (component.Type == ComponentType.Detail)
                    throw new InvalidOperationException("Detail cannot contain components.");

                var specComponent = _productRepo.Find(specificationName);
                if (specComponent == null)
                    throw new InvalidOperationException("Component not found.");

                if (multiplicity <= 0)
                    throw new ArgumentException("Multiplicity must be greater than zero.");

                var spec = new Spec(specComponent.FileOffset, multiplicity);
                _productRepo.AddSpec(component, spec);
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                    throw new InvalidOperationException("Component not found.");

                _productRepo.Delete(componentName);
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                    throw new InvalidOperationException("Component not found.");

                if (component.Type == ComponentType.Detail)
                    throw new InvalidOperationException("Detail cannot contain components.");

                var specComponent = _productRepo.Find(specificationName);
                if (specComponent == null)
                    throw new InvalidOperationException("Component not found.");

                _productRepo.DeleteSpec(component, specificationName);
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                    throw new InvalidOperationException("Component not found.");

                _productRepo.Restore(componentName);
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Restore()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.RestoreAll();
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Truncate()
        {
            try
            {
                if (!ValidateFilesOpen())
                    return;

                _productRepo.Truncate();
                OnProductsChanged();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                    throw new InvalidOperationException("Component not found.");

                if (component.Type == ComponentType.Detail)
                    throw new InvalidOperationException("Detail cannot contain components.");

                PrintComponentTree(component, "");
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
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
                OnError(ex.Message);
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

        public IEnumerable<Product> GetAllProducts()
        {
            try
            {
                if (!_filesOpen)
                    throw new InvalidOperationException("Files not open.");

                return _productRepo.GetAll();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
                return Enumerable.Empty<Product>();
            }
        }

        public Product GetProduct(string productName)
        {
            try
            {
                if (!_filesOpen)
                    throw new InvalidOperationException("Files not open.");

                return _productRepo.Find(productName);
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
                return null;
            }
        }

        public IEnumerable<Spec> GetProductSpecifications(string productName)
        {
            try
            {
                if (!_filesOpen)
                    throw new InvalidOperationException("Files not open.");

                var product = _productRepo.Find(productName);
                if (product == null)
                    throw new InvalidOperationException("Product not found.");

                return _productRepo.GetSpecsForProduct(product.FileOffset)
                    .Where(s => !s.IsDeleted);
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
                return Enumerable.Empty<Spec>();
            }
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
                    OnError(ex.Message);
                }
            }
        }

        private bool ValidateFilesOpen()
        {
            if (!_filesOpen)
            {
                OnError("Files not open.");
                return false;
            }
            return true;
        }

        private void PrintComponentTree(Product component, string prefix, bool printName = true)
        {
            if (printName)
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
                        string newPrefix = prefix + (i == specList.Count - 1 ? "   " : "│  ");
                        PrintComponentTree(child, newPrefix, false);
                    }
                }
            }
        }

        private void OnProductsChanged()
        {
            ProductsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnError(string message)
        {
            ErrorOccurred?.Invoke(this, message);
        }
    }
}
