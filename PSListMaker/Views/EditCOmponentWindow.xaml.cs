using PSListMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PSListMaker.Views
{
    /// <summary>
    /// Логика взаимодействия для EditCOmponentWindow.xaml
    /// </summary>
    public partial class EditComponentWindow : Window
    {
        private EditComponentWindowViewModel _viewModel;

        public EditComponentWindow(EditComponentWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
        }
    }
}
