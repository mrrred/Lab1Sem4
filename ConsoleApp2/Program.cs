using System;
using System.IO;
using System.Text;

namespace ConsoleApp21
{
    public class PS_File
    {

    }

    public class Product_Signature
    {
        private short _length;
        private int _first;
        private int _unclaimed;
        private char[] _spec_name = new char[16];
        private string _title;
    }

    public class Product
    {
        private byte _del_bit;
        private int _next_detail_product;
        private string _content;
        private int _offset = -1;
    }







    class Program
    {
        static string path = "path.txt";
        static void Main(string[] args)
        {
            if (File.Exists(path))
            {

            }
        }
    }
}
