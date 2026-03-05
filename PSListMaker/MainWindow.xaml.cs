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
using PSListMaker.WindowServices;

namespace PSListMaker
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        private IMainWindowService _windowService;

        public MainWindow(MainWindowViewModel mainWindowViewModel, 
            IMainWindowService windowService)
        {
            InitializeComponent();

            _mainWindowViewModel = mainWindowViewModel;
            _windowService = windowService;

            DataContext = _mainWindowViewModel;

            _mainWindowViewModel.RegisterOnError(PrintError);
        }

        private void DoActiveProgram()
        {
            Components.IsEnabled = true;
            Specs.IsEnabled = true;
        }

        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файлы изделий (*.prd)|*.prd";
            saveFileDialog.DefaultExt = "prd";
            saveFileDialog.Title = "Создать новый файл";

            if (saveFileDialog.ShowDialog() == true 
                || !(saveFileDialog.FileName != null || saveFileDialog.FileName != string.Empty))
            {
                string dataLengthBuffer = Microsoft.VisualBasic.Interaction.InputBox(
                    "Укажите длину записи данных:",
                    "Длина (в байтах)",
                    "20");

                short dataLength;

                if (short.TryParse(dataLengthBuffer, out dataLength))
                {
                    if (dataLength <= 0)
                    {
                        MessageBox.Show(dataLengthBuffer + " длина записи данных не может быть отрциательна или равна 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _mainWindowViewModel.CreateFile(saveFileDialog.FileName, dataLength);

                        DoActiveProgram();
                    }
                }
                else
                {
                    MessageBox.Show(dataLengthBuffer + " не является числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Файлы изделий (*.prd)|*.prd";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Title = "Выберите файл для открытия";

            if (openFileDialog.ShowDialog() == true 
                || !(openFileDialog.FileName != null || openFileDialog.FileName != string.Empty))
            {
                _mainWindowViewModel.OpenFile(openFileDialog.FileName);

                DoActiveProgram();
            }
        }

        private void Components_Button_Click(object sender, RoutedEventArgs e)
        {
            _windowService.GetComponentsListWindow().ShowDialog();
        }

        private void Specs_Button_Click(object sender, RoutedEventArgs e)
        {
            _windowService.GetSpecificationsWindow().ShowDialog();
        }

        private void PrintError(object? sender, string e)
        {
            MessageBox.Show(e, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnClosed(EventArgs e)
        {
            _mainWindowViewModel.UnRegisterOnError(PrintError);
            _mainWindowViewModel.DeleteTempFiles();
            base.OnClosed(e);
        }
    }
}