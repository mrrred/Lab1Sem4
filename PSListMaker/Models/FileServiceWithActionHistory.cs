using ConsoleApp2.Entities;
using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Controls;
using System.Linq;
using System.IO;

namespace PSListMaker.Models
{
    public class UndebleCommand
    {
        private EventHandler _undoCommand;

        public UndebleCommand(EventHandler undoCommand)
        {
            _undoCommand = undoCommand;
        }

        public void Undo()
        {
            _undoCommand?.Invoke(this, EventArgs.Empty);
        }
    }

    public interface IFileServiceWithActionHistory : IFileService
    {
        void DeleteTempFiles();

        bool IsCanUndo {  get; }

        bool IsUnsave { get; }

        void Undo();

        void Save();

        void ClearUndoBuffer();
    }

    public class FileServiceWithActionHistory : FileService, IFileServiceWithActionHistory
    {
        private Stack<UndebleCommand> _commandBuffer = [];

        private TempSaver _tempSaver;

        public bool IsCanUndo => _commandBuffer.Count > 0;

        public bool IsUnsave { get; private set; } = false;

        public void ClearUndoBuffer()
        {
            _commandBuffer.Clear();
        }

        public void Undo()
        {
            _commandBuffer.Pop().Undo();

            if (_commandBuffer.Count == 0)
            {
                IsUnsave = false;
            }
        }

        public void Save()
        {
            _commandBuffer.Clear();

            _tempSaver.SaveToOrigin();

            IsUnsave = false;
        }

        public void DeleteTempFiles()
        {
            if (_tempSaver != null)
            {
                _tempSaver.DeleteTempFile();
            }
        }

        public override void Create(string directoryPath, string productFileName, string specFileName, short dataLength)
        {
            base.Create(directoryPath, productFileName, specFileName, dataLength);

            base.Close();

            Open(Path.Combine(directoryPath, $"{productFileName}.prd"));
        }

        public override void Open(string fullProductPath)
        {
            if (_tempSaver != null)
            {
                _tempSaver.DeleteTempFile();
            }

            _tempSaver = new TempSaver(fullProductPath);
            _tempSaver.CreateTempFile();
            base.Open(_tempSaver.TempPath);
        }

        public override void Close()
        {
            base.Close();
            if (_tempSaver != null)
            {
                _tempSaver.DeleteTempFile();
                _tempSaver = null;
            }
        }

        public override void Input(string componentName, ComponentType type)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) => 
            {
                List<string> deletedItems = GetDeletedComps();

                List<(string, string)> deletedSpecs = GetDeletedSpecs(deletedItems);

                base.Restore();

                base.Delete(componentName);

                base.Truncate();

                DeleteCompAndSpecs(deletedItems, deletedSpecs);
            }
            ));

            base.Input(componentName, type);
        }

        public override void Input(string componentName, string specificationName)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                List<string> deletedItems = GetDeletedComps();

                List<(string, string)> deletedSpecs = GetDeletedSpecs(deletedItems);

                base.Restore();

                base.Delete(componentName, specificationName);

                base.Truncate();

                DeleteCompAndSpecs(deletedItems, deletedSpecs);
            }
            ));

            base.Input(componentName, specificationName);
        }

        public override void Input(string componentName, string specificationName, ushort multiplicity)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                List<string> deletedItems = GetDeletedComps();

                List<(string, string)> deletedSpecs = GetDeletedSpecs(deletedItems);

                base.Restore();

                base.Delete(componentName, specificationName);

                base.Truncate();

                DeleteCompAndSpecs(deletedItems, deletedSpecs);
            }
            ));

            base.Input(componentName, specificationName, multiplicity);
        }

        public override void Delete(string componentName)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                base.Restore(componentName);
            }
            ));

            base.Delete(componentName);
        }

        public override void Delete(string componentName, string specificationName)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                ushort m = base.GetProductSpecifications(componentName).First(x => x.Name == specificationName).Multiplicity;

                base.Input(componentName, specificationName, m);
            }
            ));

            base.Delete(componentName, specificationName);
        }

        public override void Edit(string productName, string newProductName)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                base.Edit(newProductName, productName);
            }
            ));

            base.Edit(productName, newProductName);
        }

        public override void EditSpec(string productName, string specName, ushort newMultiplicity)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                ushort oldM = base.GetProductSpecifications(productName).First(x => x.Name == specName).Multiplicity;

                base.EditSpec(productName, specName, oldM);
            }
            ));

            base.EditSpec(productName, specName, newMultiplicity);
        }

        public override void Restore()
        {
            IsUnsave = true;

            List<string> deletedItems = GetDeletedComps();
            List<(string, string)> deletedSpecs = GetDeletedSpecs(deletedItems);

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                DeleteCompAndSpecs(deletedItems, deletedSpecs);
            }
            ));

            base.Restore();
        }

        public override void Restore(string componentName)
        {
            IsUnsave = true;

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                base.Delete(componentName);
            }
            ));

            base.Restore(componentName);
        }

        public override void Truncate()
        {
            IsUnsave = true;

            List<(string, ComponentType)> deletedItems = GetDeletedCompsWithType();
            List<(string, string, ushort)> deletedSpecs = GetDeletedSpecsWithMult(deletedItems.Select(x => x.Item1).ToList());

            _commandBuffer.Push(new UndebleCommand((object? sender, EventArgs e) =>
            {
                foreach (var item in deletedItems)
                {
                    base.Input(item.Item1, item.Item2);
                    base.Delete(item.Item1);
                }

                foreach (var item in deletedSpecs)
                {
                    base.Input(item.Item1, item.Item2, item.Item3);
                    base.Delete(item.Item1, item.Item2);
                }
            }
            ));

            base.Truncate();
        }

        private List<string> GetDeletedComps()
        {
            return base.GetAllProducts()
                .Where(x => x.DelBit == 1)
                .Select(x => x.Name)
                .ToList();
        }

        private List<(string, ComponentType)> GetDeletedCompsWithType()
        {
            return base.GetAllProducts()
                .Where(x => x.DelBit == 1)
                .Select(x => (x.Name, x.Type))
                .ToList();
        }

        private List<(string, string)> GetDeletedSpecs(List<string> deletedComps)
        {
            List<(string, string)> deletedSpecs = [];

            foreach (var product in deletedComps)
            {
                foreach (var spec in GetProductSpecifications(product))
                {
                    if (spec.IsDeleted)
                    {
                        deletedSpecs.Add((product, spec.Name));
                    }
                }
            }

            return deletedSpecs;
        }

        private List<(string, string, ushort)> GetDeletedSpecsWithMult(List<string> deletedComps)
        {
            List<(string, string, ushort)> deletedSpecs = [];

            foreach (var product in deletedComps)
            {
                foreach (var spec in GetProductSpecifications(product))
                {
                    if (spec.IsDeleted)
                    {
                        deletedSpecs.Add((product, spec.Name, spec.Multiplicity));
                    }
                }
            }

            return deletedSpecs;
        }

        private void DeleteCompAndSpecs(List<string> deletedComps, List<(string, string)> deletedSpecs)
        {
            foreach (var item in deletedComps)
            {
                base.Delete(item);
            }

            foreach (var item in deletedSpecs)
            {
                base.Delete(item.Item1, item.Item2);
            }
        }
    }
}
