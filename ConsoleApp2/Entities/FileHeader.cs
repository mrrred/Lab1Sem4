using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Entities
{
    public class FileHeader
    {
        public string Signature { get; set; } = "PS";
        public short DataLength { get; set; }
        public int FirstRecPtr { get; set; } = -1;
        public int UnclaimedPtr { get; set; } = -1;
        public string SpecFName { get; set; } = string.Empty;
    }
}
