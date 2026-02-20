using System;

namespace ConsoleApp2.Data.Abstractions
{
    public static class FileStructure
    {
        public const int SIGNATURE_SIZE = 2;
        public const int DATA_LENGTH_SIZE = 2;
        public const int POINTER_SIZE = 4;
        public const int SPEC_FILENAME_SIZE = 16;
        
        public static int GetHeaderSize(bool isProductFile)
        {
            if (isProductFile)
                return SIGNATURE_SIZE + DATA_LENGTH_SIZE + POINTER_SIZE + POINTER_SIZE + SPEC_FILENAME_SIZE;
            else
                return SIGNATURE_SIZE + DATA_LENGTH_SIZE + POINTER_SIZE + POINTER_SIZE;
        }
        
        public static int HEADER_SIZE_PRODUCT => GetHeaderSize(true);
        public static int HEADER_SIZE_SPEC => GetHeaderSize(false);
    }
}
