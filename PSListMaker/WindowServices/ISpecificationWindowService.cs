using PSListMaker.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.WindowServices
{
    public interface ISpecificationWindowService
    {
        AddSpecificationWindow GetAddWindow(string componentName);
    }
}
