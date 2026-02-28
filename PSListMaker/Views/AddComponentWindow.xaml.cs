using ConsoleApp2.Entities;
using PSListMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public partial class AddComponentWindow : Window
    {
        static private bool IsAddError = false;

        private AddComponentWindowViewModel _viewModel;

        public AddComponentWindow(AddComponentWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            _viewModel.RegisterErrorHandler(ErrorProcess);
        }

        public void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            string name = ComponentName.Text;

            // Bad
            ComponentType type = CompType.Text switch
            {
                "Product" => ComponentType.Product,
                "Node" => ComponentType.Node,
                "Detail" => ComponentType.Detail
            };

            _viewModel.Add(name, type);

            if (IsAddError)
            {
                IsAddError = false;
            }
            else
            {
                Close();
            }
        }

        public void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        static private void ErrorProcess(object? sender, string message)
        {
            IsAddError = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.RegisterErrorHandler(ErrorProcess);
            base.OnClosed(e);
        }
    }
}
