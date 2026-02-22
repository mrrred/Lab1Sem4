using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ConsoleApp2.Data
{

    public class LLManager
    {
        private IFSManager _fsManager;
        private ISerializer<Product> _productSerializer;
        private ISerializer<Spec> _specSerializer;
        private SortedDictionary<Product, List<Spec>> _cache;

        public LLManager(IFSManager fsManager, ISerializer<Product> product_serializer, ISerializer<Spec> spec_serializer)
        {
            _fsManager = fsManager ?? throw new ArgumentNullException(nameof(fsManager));
            _productSerializer = product_serializer ?? throw new ArgumentNullException(nameof(product_serializer));
            _cache = new SortedDictionary<Product, List<Spec>>();
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
            _cache.Keys.RemoveAll(e => e.IsDeleted);
        }

        public T? FindByName(string name)
        {
            return _cache.FirstOrDefault(e => e.Name == name && !e.IsDeleted);
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _cache.Keys.Where(e => !e.IsDeleted);
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

        public void LoadFromFile()
        {
            _cache.Clear();
            ProductHeader header = _fsManager.ReadHeader();
            int currentOffset = header.FirstRecPtr;
            while (currentOffset != -1)
            {
                _fsManager.Seek(currentOffset);
                using (var reader = new BinaryReader(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    var product = _productSerializer.ReadFromFile(reader);
                    product.FileOffset = currentOffset;
                    _cache.Add(product, new List<Spec>());
                    currentOffset = product.NextProductPtr;
                }
            }
            foreach (var pair in _cache)
            {
                currentOffset = pair.Key.SpecPtr;
                while (currentOffset != -1)
                {
                    _fsManager.Seek(currentOffset);
                    using (var reader = new BinaryReader(_fsManager.GetStream(), System.Text.Encoding.UTF8, leaveOpen: true))
                    {
                        var spec = _specSerializer.ReadFromFile(reader);
                    }
                }

            }
        }
    }
}
