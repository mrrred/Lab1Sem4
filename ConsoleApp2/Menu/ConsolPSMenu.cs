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

    internal class CreateMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class OpenMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class InputMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class DeleteMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class RestoreMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class TruncateMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class PrintMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class HelpMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return true;
        }
    }

    internal class ExitMenuItem : IMenuItemStringeble
    {
        public string Title { get; private set; }

        public bool Invoke(string input)
        {
            return false;
        }
    }
}
