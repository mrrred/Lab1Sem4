using PSListMaker.Models;
using PSListMaker.ViewModels;
using System;
using System.Collections.Generic;
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

        public Specifications(SpecificationsViewModel specificationsViewModel)
        {
            InitializeComponent();

            _specificationsViewModel = specificationsViewModel;

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
    }
}
