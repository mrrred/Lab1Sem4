using ConsoleApp2.MenuService;
using PSListMaker.ViewModels;
using PSListMaker.WindowServices;
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

            IMainWindowService windowService = new MainWindowService(fileService);

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(fileService);
            MainWindow mainWindow = new MainWindow(mainWindowViewModel, windowService);



            if (mainWindow.ShowDialog() == true)
            {
                
            }

            Shutdown();
        }


    }

}
