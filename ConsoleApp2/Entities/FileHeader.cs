using System;

namespace ConsoleApp2.Entities
{
    public class FileHeader
    {
        public int FirstRecPtr { get; set; } = -1;
        public int UnclaimedPtr { get; set; } = -1;
    }
}
