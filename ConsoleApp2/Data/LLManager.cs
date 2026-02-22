using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ConsoleApp2.Data
{

    public class LLManager<T> : ILLManager<T> where T : class, IEntity
    {
        private IFSManager _fsManager;
        private ISerializer<T> _serializer;
        private INodeNavigator<T> _navigator;
        private FileHeader _header;
        private List<T> _cache;

        public LLManager(
            IFSManager fsManager,
            ISerializer<T> serializer,
            INodeNavigator<T> navigator)
        {
            _fsManager = fsManager ?? throw new ArgumentNullException(nameof(fsManager));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            _cache = new List<T>();
        }

        public void Initialize(FileHeader header)
        {
            _header = header ?? throw new ArgumentNullException(nameof(header));
            _cache.Clear();
        }

        public void Add(T entity, int offset)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            int lastOffset = -1;
            var allEntities = _cache.Where(e => !e.IsDeleted).ToList();
            
            if (allEntities.Count > 0)
            {
                T current = allEntities[0];
                while (_navigator.GetNextOffset(current) != -1)
                {
                    int nextOffset = _navigator.GetNextOffset(current);
                    var next = _cache.FirstOrDefault(e => e.FileOffset == nextOffset);
                    if (next == null)
                        break;
                    current = next;
                }
                lastOffset = current.FileOffset;
            }

            _fsManager.Seek(offset);
            using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
            {
                _serializer.WriteToFile(entity, writer);
            }
            _fsManager.GetStream().Flush();

            entity.FileOffset = offset;
            _cache.Add(entity);

            if (lastOffset != -1)
            {
                var lastEntity = _cache.FirstOrDefault(e => e.FileOffset == lastOffset);
                if (lastEntity != null)
                {
                    _navigator.SetNextOffset(lastEntity, offset);
                    
                    _fsManager.Seek(lastOffset);
                    using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                    {
                        _serializer.WriteToFile(lastEntity, writer);
                    }
                    _fsManager.GetStream().Flush();
                }
            }
        }

        public void Delete(int offset)
        {
            var entity = FindByOffset(offset);
            if (entity == null)
                throw new InvalidOperationException("Record not found at offset: " + offset);

            entity.MarkAsDeleted();
        }

        public void Restore(int offset)
        {
            var entity = FindByOffset(offset);
            if (entity == null)
                throw new InvalidOperationException("Record not found at offset: " + offset);

            entity.Restore();
        }

        public void RestoreAll()
        {
            foreach (var entity in _cache)
            {
                if (entity.IsDeleted)
                    entity.Restore();
            }
        }

        public void Truncate()
        {
            _cache.RemoveAll(e => e.IsDeleted);
        }

        public T? FindByName(string name)
        {
            return _cache.FirstOrDefault(e => e.Name == name && !e.IsDeleted);
        }

        public T? FindByOffset(int offset)
        {
            return _cache.FirstOrDefault(e => e.FileOffset == offset);
        }

        public IEnumerable<T> GetAll()
        {
            return _cache.Where(e => !e.IsDeleted);
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.FileOffset == -1)
                throw new InvalidOperationException("Entity must have a valid FileOffset to update");

            _fsManager.Seek(entity.FileOffset);
            using (var writer = new BinaryWriter(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
            {
                _serializer.WriteToFile(entity, writer);
            }
            _fsManager.GetStream().Flush();
        }

        public IEnumerable<T> FromOffset(int startOffset)
        {
            if (startOffset == -1)
                yield break;

            var current = FindByOffset(startOffset);
            while (current != null)
            {
                yield return current;
                int nextOffset = _navigator.GetNextOffset(current);
                current = nextOffset == -1 ? null : FindByOffset(nextOffset);
            }
        }

        public void SortAlphabetically()
        {
            var activeItems = GetAll().OrderBy(e => e.Name).ToList();

            for (int i = 0; i < activeItems.Count - 1; i++)
            {
                _navigator.SetNextOffset(activeItems[i], activeItems[i + 1].FileOffset);
            }

            if (activeItems.Count > 0)
            {
                _navigator.SetNextOffset(activeItems[activeItems.Count - 1], -1);
                _navigator.SetFirstOffset(activeItems[0].FileOffset);
            }
            else
            {
                _navigator.SetFirstOffset(-1);
            }
        }
        public void LoadFromFile()
        {
            _cache.Clear();
            int currentOffset = _navigator.GetFirstOffset();

            while (currentOffset != -1)
            {
                _fsManager.Seek(currentOffset);
                using (var reader = new BinaryReader(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    var entity = _serializer.ReadFromFile(reader);
                    entity.FileOffset = currentOffset;
                    _cache.Add(entity);
                    currentOffset = _navigator.GetNextOffset(entity);
                }
            }
        }
    }
}
