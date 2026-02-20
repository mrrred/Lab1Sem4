using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data
{

    public class SpecNodeNavigator : INodeNavigator<Spec>
    {
        private FileHeader _header;

        public SpecNodeNavigator(FileHeader header)
        {
            _header = header;
        }

        public int GetNextOffset(Spec entity) => entity.NextSpecPtr;

        public void SetNextOffset(Spec entity, int offset) => entity.NextSpecPtr = offset;

        public int GetFirstOffset() => _header.FirstRecPtr;

        public void SetFirstOffset(int offset) => _header.FirstRecPtr = offset;
    }
}
