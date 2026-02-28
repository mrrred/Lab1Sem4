using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Models
{
    public class ComponentsWithSpecs : ComponentMin
    {
        public List<ComponentsWithSpecs> Specs { get; }

        public ComponentsWithSpecs(string name, string type) : base(name, type)
        {
            Specs = new List<ComponentsWithSpecs>();
        }
    }
}
