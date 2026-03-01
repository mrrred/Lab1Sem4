using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Models
{
    public class ComponentsWithSpecs : ComponentsWithMult
    {
        public List<ComponentsWithSpecs> Specs { get; }

        public ComponentsWithSpecs(string name, string type, ushort multiplicyty, ComponentMin parent = null) : base(name, type, multiplicyty, parent)
        {
            Specs = new List<ComponentsWithSpecs>();
        }
    }
}
