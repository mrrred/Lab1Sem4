using ConsoleApp2.Entities;
using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Menu
{
    internal class ConsolPSMenu : IMenu
    {
        protected IFileService _fileService;

        private List<IMenuItemStringeble> MenuItems { get; set; } = [];

        public ConsolPSMenu(IFileService fileService)
        {
            _fileService = fileService;

            _fileService.ErrorOccurred += PrintErrorMessage;
        }

        public ConsolPSMenu(IFileService fileService, IEnumerable<IMenuItemStringeble> menuItems) : this(fileService)
        {
            MenuItems = menuItems.ToList();
        }

        public void Show()
        {
            while (true)
            {
                Console.Write("PS>");
                string? input = Console.ReadLine();

                if (input is null) continue;

                string[] inputSplit = input.Split(' ');

                (string command, string arguments) = (inputSplit[0], string.Join(' ', inputSplit.Skip(1)));

                bool statusOfProgram = true;

                // Can be optimized with a dictionary if the number of menu items is large
                foreach (IMenuItem item in MenuItems)
                {
                    if (command == item.Title)
                    {
                        statusOfProgram = item.Invoke(arguments);
                        break;
                    }

                    // Don't good idea
                    if (item == MenuItems.Last())
                    {
                        Console.WriteLine("Incorrect command");
                    }
                }

                // Can be exit code
                if (!statusOfProgram)
                {
                    Console.WriteLine("End of program");
                    break;
                }
            }
        }

        public void RegisterMenuItem(IMenuItemStringeble item)
        {
            MenuItems.Add(item);
        }

        protected void PrintErrorMessage(object? sender, string message)
        {
            Console.WriteLine(message);
        }
    }

    abstract class PSMenuItem
    {
        protected IFileService _fileService;

        protected PSMenuItem(IFileService fileService)
        {
            _fileService = fileService;
        }
    }

    internal class CreateMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Create";

        public CreateMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            var stringSplit = input.Split(' ');

            if (stringSplit.Length < 2)
            {
                Console.WriteLine("Error: not enough arguments");
                return true;
            }

            _fileService.Create(AppDomain.CurrentDomain.BaseDirectory,
                Path.GetFileNameWithoutExtension(stringSplit[0]),
                Path.GetFileNameWithoutExtension(stringSplit[0]),
                (short)(Convert.ToUInt16(stringSplit[1])));
            return true;
        }
    }

    internal class OpenMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Open";

        public OpenMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            var inputSplit = input.Split(' ');

            if (inputSplit.Count() != 1)
            {
                Console.WriteLine("Error: too many arguments");
                return true;
            }

            _fileService.Open(Path.GetFileNameWithoutExtension(inputSplit[0]) + ".prd");
            return true;
        }
    }

    internal class InputMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Input";

        public InputMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            string[] inputSplit = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (inputSplit.Length == 0)
            {
                Console.WriteLine("Error: not enough arguments");
                return true;
            }

            if (inputSplit.Length == 1)
            {
                Console.WriteLine("Error: not enough arguments");
                return true;
            }
            else if (inputSplit.Length == 2)
            {
                if (Enum.TryParse<ComponentType>(inputSplit[1], ignoreCase: true, out var type))
                {
                    _fileService.Input(inputSplit[0], type);
                }
                else
                {
                    _fileService.Input(inputSplit[0], inputSplit[1]);
                }
            }
            else if (inputSplit.Length >= 3)
            {
                string component = inputSplit[0];
                string specification = inputSplit[1];
                
                if (ushort.TryParse(inputSplit[2], out ushort multiplicity))
                {
                    _fileService.Input(component, specification, multiplicity);
                }
                else
                {
                    Console.WriteLine("Error: invalid multiplicity.");
                }
            }

            return true;
        }
    }

    internal class DeleteMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Delete";

        public DeleteMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            string[] inputSplit = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (inputSplit.Length == 1)
            {
                _fileService.Delete(inputSplit[0]);
            }
            else if (inputSplit.Length >= 2)
            {
                _fileService.Delete(inputSplit[0], inputSplit[1]);
            }
            else
            {
                Console.WriteLine("Error: not enough arguments");
            }

            return true;
        }
    }

    internal class RestoreMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Restore";

        public RestoreMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Trim() == "*")
            {
                _fileService.Restore();
            }
            else
            {
                _fileService.Restore(input.Trim());
            }

            return true;
        }
    }

    internal class TruncateMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Truncate";

        public TruncateMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            _fileService.Truncate();
            return true;
        }
    }

    internal class PrintMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Print";

        public PrintMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Trim() == "*")
            {
                _fileService.Print();
            }
            else
            {
                _fileService.Print(input.Trim());
            }

            return true;
        }
    }

    internal class HelpMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Help";

        public HelpMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _fileService.Help();
            }
            else
            {
                _fileService.Help(input.Trim());
            }

            return true;
        }
    }

    internal class ExitMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Exit";

        public ExitMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            return false;
        }
    }
}
