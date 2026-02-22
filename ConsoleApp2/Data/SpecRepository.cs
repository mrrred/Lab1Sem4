using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ConsoleApp2.Data
{
    public class SpecRepository : ISpecManaging
    {
        private IFSManager _fsManager;
        private ISerializer<Spec> _serializer;
        private ILLManager<Spec> _listManager;
        private INodeNavigator<Spec> _navigator;
        private FileHeader _header;
        private string _filePath;

        public SpecRepository(
            IFSManager fsManager,
            ISerializer<Spec> serializer,
            ILLManager<Spec> listManager,
            INodeNavigator<Spec> navigator)
        {
            _fsManager = fsManager ?? throw new ArgumentNullException(nameof(fsManager));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _listManager = listManager ?? throw new ArgumentNullException(nameof(listManager));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        public void Create(string filePath)
        {
            _filePath = filePath;
            ((FSManager)_fsManager).CreateFile();

            _header = new FileHeader
            {
                Signature = "PS",
                DataLength = 0,
                FirstRecPtr = -1,
                UnclaimedPtr = -1,
                SpecFName = string.Empty
            };

            _fsManager.WriteHeader(_header);
            _listManager.Initialize(_header);
            _navigator = new SpecNodeNavigator(_header);
        }

        public void Open(string filePath)
        {
            _filePath = filePath;
            ((FSManager)_fsManager).OpenFile();

            _header = _fsManager.ReadHeader();
            _listManager.Initialize(_header);
            _navigator = new SpecNodeNavigator(_header);
            _listManager.LoadFromFile();
        }

        public void Close()
        {
            _fsManager.Close();
        }

        public void Add(Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            int offset = CalculateNextOffset();
            _listManager.Add(spec, offset);

            if (_header.FirstRecPtr == -1)
            {
                _header.FirstRecPtr = offset;
                _navigator.SetFirstOffset(offset);
            }

            UpdateHeader();
        }

        public int AddAndGetOffset(Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            int offset = CalculateNextOffset();
            _listManager.Add(spec, offset);

            if (_header.FirstRecPtr == -1)
            {
                _header.FirstRecPtr = offset;
                _navigator.SetFirstOffset(offset);
            }

            UpdateHeader();
            return offset;
        }

        public void Delete(int specOffset)
        {
            _listManager.Delete(specOffset);
            
            _header.UnclaimedPtr = specOffset;
            
            UpdateHeader();
        }

        public void Restore(int specOffset)
        {
            _listManager.Restore(specOffset);
            UpdateHeader();
        }

        public void RestoreAll()
        {
            _listManager.RestoreAll();
            UpdateHeader();
        }

        public void Truncate()
        {
            _listManager.Truncate();
            CompactFile();
            UpdateHeader();
        }

        public Spec? FindByOffset(int offset)
        {
            return _listManager.FindByOffset(offset);
        }

        public IEnumerable<Spec> GetByProductOffset(int productOffset)
        {
            return _listManager.GetAll().Where(s => s.ComponentPtr == productOffset);
        }

        public IEnumerable<Spec> GetByComponentPtr(int componentOffset)
        {
            return _listManager.GetAll().Where(s => s.ComponentPtr == componentOffset);
        }

        public IEnumerable<Spec> GetAll()
        {
            return _listManager.GetAll();
        }

        public void Update(Spec spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            _listManager.Update(spec);
            UpdateHeader();
        }

        private void UpdateHeader()
        {
            _fsManager.WriteHeader(_header);
            _fsManager.GetStream().Flush();
        }

        private int CalculateNextOffset()
        {
            if (_header.UnclaimedPtr != -1)
            {
                int targetOffset = _header.UnclaimedPtr;
                
                _fsManager.Seek(targetOffset);
                using (var reader = new BinaryReader(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    reader.ReadByte();
                    int nextUnclaimed = reader.ReadInt32();
                    _header.UnclaimedPtr = nextUnclaimed;
                }
                
                return targetOffset;
            }
            
            long fileSize = _fsManager.GetStream().Length;
            if (fileSize == 0)
                return FileStructure.HEADER_SIZE_SPEC;
            
            return (int)fileSize;
        }

        private void CompactFile()
        {
            var activeSpecs = _listManager.GetAll().ToList();
            
            if (activeSpecs.Count == 0)
            {
                _header.FirstRecPtr = -1;
                _header.UnclaimedPtr = -1;
                return;
            }
            
            string tempPath = _filePath + ".tmp";
            
            using (var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite))
            {
                int currentOffset = FileStructure.HEADER_SIZE_SPEC;
                int firstOffset = currentOffset;
                
                // Write header to temp file
                using (var headerWriter = new BinaryWriter(tempStream, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    tempStream.Seek(0, SeekOrigin.Begin);
                    _header.FirstRecPtr = firstOffset;
                    _header.UnclaimedPtr = -1;
                    
                    headerWriter.Write(System.Text.Encoding.ASCII.GetBytes(_header.Signature.PadRight(2).Substring(0, 2)));
                    headerWriter.Write(_header.DataLength);
                    headerWriter.Write(_header.FirstRecPtr);
                    headerWriter.Write(_header.UnclaimedPtr);
                }
                
                // Write records
                for (int i = 0; i < activeSpecs.Count; i++)
                {
                    var spec = activeSpecs[i];
                    spec.FileOffset = currentOffset;
                    
                    if (i < activeSpecs.Count - 1)
                    {
                        spec.NextSpecPtr = currentOffset + _serializer.GetEntitySize();
                    }
                    else
                    {
                        spec.NextSpecPtr = -1;
                    }
                    
                    tempStream.Seek(currentOffset, SeekOrigin.Begin);
                    using (var writer = new BinaryWriter(tempStream, System.Text.Encoding.UTF8, leaveOpen: true))
                    {
                        _serializer.WriteToFile(spec, writer);
                    }
                    
                    currentOffset += _serializer.GetEntitySize();
                }
                
                tempStream.Flush();
            }
            
            // Close main file, replace it with temp file
            _fsManager.Close();
            File.Delete(_filePath);
            File.Move(tempPath, _filePath);
            
            // Reopen the file
            ((FSManager)_fsManager).OpenFile();
        }
    }
}
