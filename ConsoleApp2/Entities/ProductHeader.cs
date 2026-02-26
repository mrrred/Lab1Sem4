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
            private set { _signature = value; }
        }

        public short DataLength
        {
            get { return _dataLength; }
            private set { 
                if (value <= 0 || value > 32000) 
                    throw new ArgumentOutOfRangeException(nameof(value), "Data length must be between 1 and 32000.");
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
            set { _unclaimedPtr = value; }
        }

        public string SpecFileName
        {
            get { return _specFileName; }
            private set { _specFileName = value; }
        }

        public ProductHeader(short dataLength, string specFileName)
        {
            SetDataLength(dataLength);
            Signature = FileStructure.DEFAULT_SIGNATURE;
            _firstRecPtr = -1;
            _unclaimedPtr = -1;
            SetSpecFileName(specFileName);
        }

        public ProductHeader(string signature, short dataLength, int firstRecPtr, int unclaimedPtr, string specFileName)
        {
            if (string.IsNullOrEmpty(signature))
                throw new ArgumentException("Signature cannot be null or empty.");

            Signature = signature;
            SetDataLength(dataLength);
            _firstRecPtr = firstRecPtr;
            _unclaimedPtr = unclaimedPtr;
            SetSpecFileName(specFileName);
        }

        public void SetDataLength(short dataLength)
        {
            DataLength = dataLength;
        }

        public void SetFirstRecPtr(int firstRecPtr)
        {
            FirstRecPtr = firstRecPtr;
        }

        public void SetUnclaimedPtr(int unclaimedPtr)
        {
            UnclaimedPtr = unclaimedPtr;
        }

        public void SetSpecFileName(string specFileName)
        {
            if (string.IsNullOrWhiteSpace(specFileName))
                throw new ArgumentException("Specification file name cannot be null or empty.");
            SpecFileName = specFileName;
        }

        public static int GetHeaderSize()
        {
            return FileStructure.SIGNATURE_SIZE + FileStructure.DATA_LENGTH_SIZE + FileStructure.POINTER_SIZE + FileStructure.POINTER_SIZE + FileStructure.SPEC_FILENAME_SIZE;
        }
    }
}
