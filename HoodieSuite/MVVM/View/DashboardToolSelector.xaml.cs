using HoodieSuite.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HoodieSuite.MVVM.View
{
    /// <summary>
    /// Interaction logic for DashboardToolSelector.xaml
    /// </summary>
    public partial class DashboardToolSelector : UserControl
    {
        public DashboardViewModel ViewModel
        {
            get
            {
                if (this.DataContext != null)
                    return this.DataContext as DashboardViewModel;
                else
                    return null;
            }
        }

        public DashboardToolSelector()
        {
            InitializeComponent();
        }

        private void GameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tools.ItemsSource = ViewModel.GameToolPage[ViewModel.SelectedGame];
        }
    }
}
