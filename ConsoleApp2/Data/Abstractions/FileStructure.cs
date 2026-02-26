using System;

namespace ConsoleApp2.Data.Abstractions
{
    public static class FileStructure
    {
        public const string DEFAULT_SIGNATURE = "PS";
        public const int SIGNATURE_SIZE = 2;
        public const int DATA_LENGTH_SIZE = 2;
        public const int POINTER_SIZE = 4;
        public const int SPEC_FILENAME_SIZE = 16;
        public const int DELBIT_SIZE = 1;
        public const int MULTIPLICITY_SIZE = 2;
        public const int TYPE_SIZE = 1;
    }
}
