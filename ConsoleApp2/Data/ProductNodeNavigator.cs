using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data
{

    public class ProductNodeNavigator : INodeNavigator<Product>
    {
        private FileHeader _header;

        public ProductNodeNavigator(FileHeader header)
        {
            _header = header;
        }

        public int GetNextOffset(Product entity) => entity.NextProductPtr;

        public void SetNextOffset(Product entity, int offset) => entity.NextProductPtr = offset;

        public int GetFirstOffset() => _header.FirstRecPtr;

        public void SetFirstOffset(int offset) => _header.FirstRecPtr = offset;
    }
}
