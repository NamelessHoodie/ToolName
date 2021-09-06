using HoodieSuite.MVVM.ViewModel;
using HoodieSuite.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
            var LastSelectedGame = ResourcesRW.ReadKeyFromResourceFile("Last_Selected_Game");
            GamesListBox.SelectedItem = LastSelectedGame;
            tools.ItemsSource = ViewModel.GameToolPage[LastSelectedGame];
        }

        private void GameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tools.ItemsSource = ViewModel.GameToolPage[ViewModel.SelectedGame];
            ResourcesRW.AddOrUpdateResource("Last_Selected_Game", ViewModel.SelectedGame);
        }

        private void Double_Click_LaunchTool(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                Process proc = Process.Start(ViewModel.SelectedTool.ToolPath);
                Debug.WriteLine($"Launching: {ViewModel.SelectedTool.ToolPath}");
            }
        }
    }
}
