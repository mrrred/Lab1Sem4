using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Data.Abstractions
{
    public interface INodeNavigator<T> where T : class
    {
        int GetNextOffset(T entity);
        void SetNextOffset(T entity, int offset);
        int GetFirstOffset();
        void SetFirstOffset(int offset);
    }
}
