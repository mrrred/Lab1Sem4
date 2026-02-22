using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp2.Data.Abstractions;

namespace ConsoleApp2.Entities
{
    public class SpecHeader
    {
        private int _firstRecPtr;
        private int _unclaimedPtr;
        public int FirstRecPtr
        {
            get { return _firstRecPtr; }
            set { _firstRecPtr = value; }
        }
        public int UnclaimedPtr
        {
            get { return _unclaimedPtr; }
            set { _unclaimedPtr = value; }
        }

        public SpecHeader(int firstRecPtr, int unclaimedPtr)
        {
            FirstRecPtr = firstRecPtr;
            UnclaimedPtr = unclaimedPtr;
        }

        public static int GetHeaderSize()
        {
            return FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE;
        }
    }
}
