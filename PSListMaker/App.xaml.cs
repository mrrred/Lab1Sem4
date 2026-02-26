using ConsoleApp2.MenuService;
using PSListMaker.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PSListMaker
{
    //IoC container

    public partial class App : Application
    {
        public App()
        {
            IFileService fileService = new FileService();

            ComponentsListViewModel componentsListViewModel = new ComponentsListViewModel(fileService);
            ComponentsList componenstList = new ComponentsList(componentsListViewModel);

            SpecificationsViewModel specificationsViewModel = new SpecificationsViewModel(fileService);
            Specifications specifications = new Specifications(specificationsViewModel);

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(fileService);
            MainWindow mainWindow = new MainWindow(mainWindowViewModel, componenstList, specifications);



            if (mainWindow.ShowDialog() == true)
            {
                
            }

            Shutdown();
        }


    }

}
