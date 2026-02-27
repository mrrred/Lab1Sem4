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
using PSListMaker.WindowServices;

namespace PSListMaker
{
    public partial class ComponentsList : Window
    {
        private ComponentsListViewModel _componentsListViewModel;

        private IComponentListService _componentListService;

        public ComponentsList(IComponentListService componentListService, ComponentsListViewModel componentsListViewModel)
        {
            InitializeComponent();

            _componentsListViewModel = componentsListViewModel;
            _componentListService = componentListService;

            CompNameTypeList.ItemsSource = _componentsListViewModel.GetComponents();

            _componentsListViewModel.RegisterOnChange(Update);
        }

        public void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            _componentListService.GetAddWindow().ShowDialog();
        }

        public void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            _componentListService.GetEditWindow().ShowDialog();
        }

        public void Update(object? sender, EventArgs e)
        {
            CompNameTypeList.ItemsSource = _componentsListViewModel.GetComponents();
        }
    }
}
