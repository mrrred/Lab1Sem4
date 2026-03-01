using ConsoleApp2.MenuService;
using PSListMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
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
    
    public partial class AddSpecificationWindow : Window
    {
        private AddSpecificationWindowViewModel _viewModel;

        private string _componentName;

        public AddSpecificationWindow(string componentName, AddSpecificationWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _componentName = componentName;

            UpdateItemSource();

            _viewModel.RegisterOnChange((sender, args) => UpdateItemSource());
        }

        public void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SpecAddVar.SelectedItem != null)
            {
                ushort multiplicity = Convert.ToUInt16(SpecAddValue.Text);

                _viewModel.AddSpecification((SpecAddVar.SelectedItem as string) ?? "", multiplicity);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a product to add a specification to.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Muliplicity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ushort.TryParse(SpecAddValue.Text, out ushort multiplicity))
            {
                if (multiplicity < 1)
                {
                    SpecAddValue.Text = "1";
                }
            }
            else
            {
                SpecAddValue.Text = "1";
            }
        }

        private void UpdateItemSource()
        {
            SpecAddVar.ItemsSource = _viewModel.GetAllProductNames();
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.UnRegisterOnChange((sender, args) => UpdateItemSource());

            base.OnClosed(e);
        }

    }
}
