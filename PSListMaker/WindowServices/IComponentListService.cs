using PSListMaker.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.WindowServices
{
    public interface IComponentListService
    {
        AddComponentWindow GetAddWindow();

        EditComponentWindow GetEditWindow(string oldComponentName);
    }
}
