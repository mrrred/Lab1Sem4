using PSListMaker.Models;
using PSListMaker.ViewModels;
using PSListMaker.WindowServices;
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

namespace PSListMaker
{
    public partial class Specifications : Window
    {
        private SpecificationsViewModel _specificationsViewModel;
        private ISpecificationWindowService _specificationWindowService;

        public Specifications(ISpecificationWindowService specificationWindowService, 
            SpecificationsViewModel specificationsViewModel)
        {
            InitializeComponent();

            _specificationsViewModel = specificationsViewModel;
            _specificationWindowService = specificationWindowService;

            DataContext = _specificationsViewModel;
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }

        public void AddSpecs_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Components.SelectedItem is ComponentMin selectedComponent)
            {
                _specificationWindowService.GetAddWindow(selectedComponent.Name).ShowDialog();
            }
        }

        public void Change_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Components.SelectedItem is ComponentsWithMult selectedComponent)
            {
                _specificationWindowService.GetChangeMultiplicityWindow(
                    selectedComponent?.Parent?.Name ?? "", selectedComponent?.Name ?? "")
                    .ShowDialog();
            }
        }

        public void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Components.SelectedItem is ComponentsWithMult selectedComponent)
            {
                _specificationsViewModel.RemoveSpecs(selectedComponent?.Parent?.Name ?? "", selectedComponent?.Name ?? "");
            }
        }

        public void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        protected override void OnClosed(EventArgs e)
        {
            _specificationsViewModel.UnRegister();

            base.OnClosed(e);
        }
    }
}
