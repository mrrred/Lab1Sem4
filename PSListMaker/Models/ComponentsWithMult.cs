using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Models
{
    public class ComponentsWithMult : ComponentMin
    {
        public ushort Multiplicity { get; }

        public ComponentMin? Parent { get; }

        public string NameWithMultString
        {
            get 
            { 
                if (Multiplicity == 0)
                {
                    return Name;
                }

                return $"{Name} ({Multiplicity}x)"; 
            }
        }


        public ComponentsWithMult(string name, string type, ushort multiplicity, ComponentMin? parent = null) : base(name, type)
        {
            Multiplicity = multiplicity;
            Parent = parent;
        }
    }
}
