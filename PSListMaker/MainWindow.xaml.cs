using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PSListMaker.ViewModels;

using Microsoft.Win32;

namespace PSListMaker
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private ComponentsList _componentsList;
        private Specifications _specifications;

        public MainWindow(MainWindowViewModel mainWindowViewModel, 
            ComponentsList componentsList, 
            Specifications specifications)
        {
            InitializeComponent();

            _mainWindowViewModel = mainWindowViewModel;
            _componentsList = componentsList;
            _specifications = specifications;

            DataContext = _mainWindowViewModel;
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Файлы изделий (*.prd)|*.prd";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Title = "Выберите файл для открытия";

            if (openFileDialog.ShowDialog() == true)
            {
                _mainWindowViewModel.OpenFile(openFileDialog.FileName);
            }
        }

        private void Components_Button_Click(object sender, RoutedEventArgs e)
        {
            _componentsList.ShowDialog();
        }

        private void Specs_Button_Click(object sender, RoutedEventArgs e)
        {
            _specifications.ShowDialog();
        }
    }
}