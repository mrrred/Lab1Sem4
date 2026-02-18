using ConsoleApp2.Menu;
using System;
using System.IO;
using System.Text;

namespace ConsoleApp21
{
    public interface IFile
    {
        void Add(object content);
        void Pre_Delete(int offset);
        void Fully_Delete(int offset);
        void Restore(int offset);
    }

    public class PS_File
    {
        private string path = "list.prd";
        private Product_Signature _header;
        public PS_File()
        {
            if (!File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    _header = new Product_Signature();
                    WriteHeader(writer, _header);
                }
                Console.WriteLine("Файл создан.");
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    try
                    {
                        _header = ReadHeader(reader);
                        Console.WriteLine("Файл успешно открыт и заголовок прочитан.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
                    }
                }
            }
        }

        private void WriteHeader(BinaryWriter writer, Product_Signature header)
        {
            writer.Write(Encoding.ASCII.GetBytes("PS"));
            writer.Write(header.Length);
            writer.Write(header.First);
            writer.Write(header.Unclaimed);
            char[] nameBuffer = new char[16];
            Array.Copy(header.Spec_Name, nameBuffer, Math.Min(header.Spec_Name.Length, 16));
            writer.Write(nameBuffer);
            writer.Write(header.Title);
        }



        private Product_Signature ReadHeader(BinaryReader reader)
        {

            return new Product_Signature();
        }

    }

    public class Product_Signature
    {
        private short _length = 22;
        private int _first = -1;
        private int _unclaimed = -1;
        private char[] _spec_name = new char[16];
        private string _title = "list.prs";

        public short Length
        {
            get { return _length; }
        }

        public int First
        {
            get { return _first; }
        }

        public int Unclaimed
        {
            get { return _unclaimed; }
        }

        public char[] Spec_Name
        {
            get { return _spec_name; }
        }

        public string Title
        {
            get { return _title; }
        }

    }

    public class Product
    {
        private byte _del_bit;
        private int _next_product;
        private string _content;
        private int _offset = -1;
    }

    interface IType
    {

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            IMenu menu = new ConsolPSMenu(new List<IMenuItemStringeble>
            {
                new CreateMenuItem(),
                new OpenMenuItem(),
                new InputMenuItem(),
                new DeleteMenuItem(),
                new RestoreMenuItem(),
                new TruncateMenuItem(),
                new PrintMenuItem(),
                new HelpMenuItem(),
                new ExitMenuItem()
            });

            menu.Show();
        }
    }
}
