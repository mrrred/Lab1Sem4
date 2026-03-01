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

namespace PSListMaker.Views
{
    public partial class ChangeMultiplicityWindow : Window
    {
        private ChangeMultiplicityWindowViewModel _viewModel;

        public ChangeMultiplicityWindow(ChangeMultiplicityWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
        }

        public void Muliplicity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ushort.TryParse(MultValue.Text, out ushort multiplicity))
            {
                if (multiplicity < 1)
                {
                    MultValue.Text = "1";
                }
            }
            else
            {
                MultValue.Text = "1";
            }
        }

        public void Change_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ushort.TryParse(MultValue.Text, out ushort multiplicity))
            {
                if (multiplicity < 1)
                {
                    multiplicity = 1;
                }
            }
            else
            {
                multiplicity = 1;
            }
            _viewModel.ChangeMultiplicity(multiplicity);
            Close();
        }

        public void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
