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

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var original = e.OriginalSource as DependencyObject;
            var item = FindAncestor<TreeViewItem>(original);
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                e.Handled = true;
            }
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
