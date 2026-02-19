using ConsoleApp2.Menu;
using ConsoleApp2.MenuService;
using ConsoleApp2.Menu;
using System;
using System.IO;
using System.Text;

namespace ConsoleApp21
{
    public interface IFile
    {
        void Add(object content);
        void Pre_Delete(object content);
        void Fully_Delete(object content);
        void Restore(object content);
    }

    public class PS_File : IFile
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
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    try
                    {
                        _header = ReadHeader(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
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
            byte[] signature = reader.ReadBytes(2);
            string sigString = Encoding.ASCII.GetString(signature);
            if (sigString != "PS")
            {
                throw new Exception("отсутствует сигнатура PS");
            }
            Product_Signature header = new Product_Signature();
            short length = reader.ReadInt16();
            int first = reader.ReadInt32();
            int unclaimed = reader.ReadInt32();
            char[] spec_name = reader.ReadChars(16);
            string title = reader.ReadString();
            header.SetData(length, first, unclaimed, spec_name, title);
            return header;
        }

        public void Add(object content)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryWriter writer = new BinaryWriter(fs))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int targetOffset;
                if (_header.Unclaimed != -1)
                {
                    targetOffset = _header.Unclaimed;
                    fs.Seek(targetOffset + 1, SeekOrigin.Begin);
                    int nextUnclaimed = reader.ReadInt32();
                    _header.SetUnclaimed(nextUnclaimed);
                    fs.Seek(8, SeekOrigin.Begin);
                    writer.Write(_header.Unclaimed);
                }
                else
                {
                    targetOffset = (int)fs.Length;
                }
                fs.Seek(targetOffset, SeekOrigin.Begin);
                writer.Write((byte)0);
                writer.Write(_header.First);
                writer.Write(content.ToString());
                _header.SetFirst(targetOffset);
                fs.Seek(4, SeekOrigin.Begin);
                writer.Write(_header.First);
            }
        }

        public void Pre_Delete(object content)
        {
            int offset = Find(content.ToString());
            if (offset == -1) return;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                writer.Write((byte)1);
            }
        }

        public void Fully_Delete(object content)
        {
            string target = content.ToString();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fs))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                int current = _header.First;
                int prev = -1;
                while (current != -1)
                {
                    fs.Seek(current, SeekOrigin.Begin);
                    byte status = reader.ReadByte();
                    int next = reader.ReadInt32();
                    string c = reader.ReadString();
                    if (c == target)
                    {
                        if (prev == -1)
                        {
                            _header.SetFirst(next);
                            fs.Seek(4, SeekOrigin.Begin);
                            writer.Write(_header.First);
                        }
                        else
                        {
                            fs.Seek(prev + 1, SeekOrigin.Begin);
                            writer.Write(next);
                        }


                        fs.Seek(current, SeekOrigin.Begin);
                        writer.Write((byte)2);
                        writer.Write(_header.Unclaimed);

                        _header.SetUnclaimed(current);
                        fs.Seek(8, SeekOrigin.Begin);
                        writer.Write(_header.Unclaimed);

                        return;
                    }
                    prev = current;
                    current = next;
                }
            }
        }
        public void Restore(object content)
        {
            string target = content.ToString();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fs))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                int current = _header.First;
                while (current != -1)
                {
                    fs.Seek(current, SeekOrigin.Begin);
                    byte status = reader.ReadByte();
                    int next = reader.ReadInt32();
                    string c = reader.ReadString();

                    if (status == 1 && c == target)
                    {
                        fs.Seek(current, SeekOrigin.Begin);
                        writer.Write((byte)0);
                        return;
                    }
                    current = next;
                }
            }
        }

        public int Find(string contentToFind)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int currentOffset = _header.First;
                while (currentOffset != -1)
                {
                    fs.Seek(currentOffset, SeekOrigin.Begin);
                    byte delBit = reader.ReadByte();
                    int nextOffset = reader.ReadInt32();
                    string content = reader.ReadString();
                    if (delBit == 0 && content == contentToFind)
                    {
                        return currentOffset;
                    }
                    currentOffset = nextOffset;
                }
            }
            return -1;
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

        public void SetData(short length, int first, int unclaimed, char[] spec_name, string title)
        {
            _length = length;
            _first = first;
            _unclaimed = unclaimed;
            _spec_name = spec_name;
            _title = title;
        }
        public void SetFirst(int offset) => _first = offset;
        public void SetUnclaimed(int offset) => _unclaimed = offset;
    }

    public class Product
    {
        private byte _del_bit;
        private int _next_product;
        private string _content;
        private int _offset = -1;
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            var fileServ = new FileService();

            IMenu menu = new ConsolPSMenu(new List<IMenuItemStringeble>
            {
                new CreateMenuItem(fileServ),
                new OpenMenuItem(fileServ),
                new InputMenuItem(fileServ),
                new DeleteMenuItem(fileServ),
                new RestoreMenuItem(fileServ),
                new TruncateMenuItem(fileServ),
                new PrintMenuItem(fileServ),
                new HelpMenuItem(fileServ),
                new ExitMenuItem()
            });

            menu.Show();
        }
    }
}
