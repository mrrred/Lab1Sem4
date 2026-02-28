using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Models
{
    public class ComponentsWithSpecs
    {
        public string Name { get; }

        public List<ComponentsWithSpecs> Specs { get; }

        public ComponentsWithSpecs(string name)
        {
            Name = name;
            Specs = new List<ComponentsWithSpecs>();
        }
    }
}
