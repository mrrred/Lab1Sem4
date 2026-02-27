using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.WindowServices
{
    public interface IMainWindowService
    {
        ComponentsList GetComponentsListWindow();

        Specifications GetSpecificationsWindow();
    }
}
