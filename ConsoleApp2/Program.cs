using ConsoleApp2.MenuService;
using ConsoleApp2.Menu;
using System;
using System.Collections.Generic;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IFileService fileService = new FileService();

            IMenu menu = new ConsolPSMenu(new List<IMenuItemStringeble>
            {
                new CreateMenuItem(fileService),
                new OpenMenuItem(fileService),
                new InputMenuItem(fileService),
                new DeleteMenuItem(fileService),
                new RestoreMenuItem(fileService),
                new TruncateMenuItem(fileService),
                new PrintMenuItem(fileService),
                new HelpMenuItem(fileService),
                new ExitMenuItem(fileService)
            });

            menu.Show();
        }
    }
}
