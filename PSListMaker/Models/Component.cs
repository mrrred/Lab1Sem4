using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Models
{
    public class ComponentMin
    {
        public string Name { get; }

        public string Type { get; }

        public ComponentMin(string name, string type)
        {
            Name = name;
            Type = type;
        }

    }
}
