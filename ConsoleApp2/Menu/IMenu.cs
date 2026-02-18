using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2.Menu
{
    internal interface IMenu
    {
        void Show();
    }

    internal interface IMenuItem : InvokeWithStringInput
    {
        public string Title { get; }
    }

    internal interface InvokeWithStringInput
    {
        public bool Invoke(string input);
    }

    internal interface IMenuItemStringeble : IMenuItem, InvokeWithStringInput { }
}
