using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Menu
{
    internal class ConsolPSMenu : IMenu
    {
        private List<IMenuItemStringeble> MenuItems { get; set; }

        public ConsolPSMenu()
        {
            MenuItems = [];
        }

        public ConsolPSMenu(IEnumerable<IMenuItemStringeble> menuItems)
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
            Console.WriteLine(Title);

            _fileService.Create(input);

            return true;
        }
    }

    internal class OpenMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Open";

        public OpenMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            Console.WriteLine(Title);

            _fileService.Open(input);

            return true;
        }
    }

    internal class InputMenuItem : PSMenuItem, IMenuItemStringeble
    {
        public string Title { get; private set; } = "Input";

        public InputMenuItem(IFileService fileService) : base(fileService) { }

        public bool Invoke(string input)
        {
            Console.WriteLine(Title);

            string[] inputSplit = input.Split(' ');

            if (inputSplit.Length == 0)
            {
                throw new ArgumentException("Not enough arguments");
            }

            if (inputSplit.Length == 1)
            {
                _fileService.Input(inputSplit[0], string.Join(' ', inputSplit.Skip(1)));
            }
            else
            {
                switch (inputSplit[1])
                {
                    case "Product":
                        _fileService.Input(inputSplit[0], ComponentType.Product);
                        break;
                    case "Unit":
                        _fileService.Input(inputSplit[0], ComponentType.Unit);
                        break;
                    case "Detail":
                        _fileService.Input(inputSplit[0], ComponentType.Detail);
                        break;
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
            Console.WriteLine(Title);

            string[] inputSplit = input.Split(' ');

            if (inputSplit.Length == 1)
            {
                _fileService.Delete(inputSplit[0]);
            }
            else
            {
                _fileService.Delete(inputSplit[0], inputSplit[1]);
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
            Console.WriteLine(Title);

            string[] inputSplit = input.Split(' ');

            if (inputSplit[0] == "*")
            {
                _fileService.Restore();
            }
            else
            {
                _fileService.Restore(inputSplit[0]);
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
            Console.WriteLine(Title);

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
            Console.WriteLine(Title);

            string[] inputSplit = input.Split(' ');

            if (inputSplit[1] == "*")
            {
                _fileService.Print();
            }
            else
            {
                _fileService.Print(inputSplit[0]);
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
            Console.WriteLine(Title);

            string[] inputSplit = input.Split(' ');

            if (inputSplit.Length == 0)
            {
                _fileService.Help();
            }
            else
            {
                _fileService.Help(inputSplit[0]);
            }

            return true;
        }
    }

    internal class ExitMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; } = "Exit";

        public bool Invoke(string input)
        {
            Console.WriteLine(Title);

            return false;
        }
    }
}
