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

using ConsoleApp2.MenuService;
using PSListMaker.ViewModels;

namespace PSListMaker
{
    public partial class ComponentsList : Window
    {
        private ComponentsListViewModel _componentsListViewModel;

        public ComponentsList(ComponentsListViewModel componentsListViewModel)
        {
            InitializeComponent();

            _componentsListViewModel = componentsListViewModel;

            CompNameTypeList.ItemsSource = _componentsListViewModel.GetComponents();
        }
    }
}
