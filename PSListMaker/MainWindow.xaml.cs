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

namespace PSListMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Components_Button_Click(object sender, RoutedEventArgs e)
        {
            new ComponenstList().ShowDialog();
        }

        private void Specs_Button_Click(object sender, RoutedEventArgs e)
        {
            new Specifications().ShowDialog();
        }
    }
}