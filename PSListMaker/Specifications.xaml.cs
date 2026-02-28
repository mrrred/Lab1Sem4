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

            FormTree();
        }

        private void FormTree()
        {
            var a = _specificationsViewModel.GetComponentsTree();

            foreach (var component in a)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = component.Name;

                FormTreeRecursive(component.Specs, item);

                Components.Items.Add(item);
            }
        }

        private void FormTreeRecursive(List<ComponentsWithSpecs> components, TreeViewItem item)
        {
            foreach (var component in components)
            {
                TreeViewItem itemNew = new TreeViewItem();
                itemNew.Header = component.Name;

                FormTreeRecursive(component.Specs, itemNew);

                item.Items.Add(itemNew);
            }
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as TreeViewItem;

            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }
    }
}
