using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConsoleApp2.Entities;

namespace ConsoleApp2.Data.Abstractions
{
    public interface IFSManager
    {
        FileStream GetStream();
        void WriteHeader(ProductHeader header);
        FileHeader ReadHeader();
        void Seek(long offset);
        long GetPosition();
        void Close();
    }
}
