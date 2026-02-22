using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp2.Data.Abstractions;

namespace ConsoleApp2.Entities
{
    public class ProductHeader
    {
        private string _signature;
        private short _dataLength;
        private int _firstRecPtr;
        private int _unclaimedPtr;
        private string _specFileName;

        public string Signature
        {
            get { return _signature; }
            set { _signature = value; }
        }
        public short DataLength
        {
            get { return _dataLength; }
            private set { 
                if (value <= 0 || value > 32000) throw new ArgumentOutOfRangeException("value should be less than 32000 and more than 0");
                _dataLength = value; 
            }
        }
        public int FirstRecPtr
        {
            get { return _firstRecPtr; }
            set { _firstRecPtr = value; }
        }
        public int UnclaimedPtr
        {
            get { return _unclaimedPtr; }
            set { _unclaimedPtr = value;}
        }
        public string SpecFileName
        {
            get
            {
                return _specFileName;
            }

            set { _specFileName = value; }
        }

        public ProductHeader(short dataLength, string specFileName)
        {
            Signature = FileStructure.DEFAULT_SIGNATURE;
            DataLength = dataLength;
            FirstRecPtr = -1;
            UnclaimedPtr = -1;
            SpecFileName = specFileName;
        }

        public ProductHeader(string signature, short dataLength, int firstRecPtr, int unclaimedPtr, string specFileName)
        {
            Signature = signature;
            DataLength = dataLength;
            FirstRecPtr = firstRecPtr;
            UnclaimedPtr = unclaimedPtr;
            SpecFileName = specFileName;
        }

        public static int GetHeaderSize()
        {
            return FileStructure.SIGNATURE_SIZE + FileStructure.DATA_LENGTH_SIZE + FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE + FileStructure.SPEC_FILENAME_SIZE;
        }
    }
}
