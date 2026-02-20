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

        private void UpdateHeader()
        {
            _fsManager.WriteHeader(_header);
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
            
            int currentOffset = FileStructure.HEADER_SIZE_SPEC;
            int firstOffset = currentOffset;
            
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
                
                _fsManager.Seek(currentOffset);
                using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    _serializer.WriteToFile(spec, writer);
                }
                
                currentOffset += _serializer.GetEntitySize();
            }
            
            _header.FirstRecPtr = firstOffset;
            _header.UnclaimedPtr = -1;
            
            _fsManager.Seek(0);
            using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
            {
                _fsManager.WriteHeader(_header);
            }
        }
    }
}
