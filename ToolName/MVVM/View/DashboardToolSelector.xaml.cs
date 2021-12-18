using ToolName.MVVM.Model;
using ToolName.MVVM.ViewModel;
using ToolName.Properties;
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

namespace ToolName.MVVM.View
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
            if (ViewModel.GameToolPage.TryGetValue(LastSelectedGame, out Dictionary<string, ToolEntry> gameToolsDictionary))
            {
                tools.ItemsSource = gameToolsDictionary.Values;
            }
        }

        private void GameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tools.ItemsSource = ViewModel.GameToolPage[ViewModel.SelectedGame].Values;
            ResourcesRW.AddOrUpdateResource("Last_Selected_Game", ViewModel.SelectedGame);
        }

        private void Double_Click_LaunchTool(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)   
            {
                var toolsDirectoryPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                ViewModel.SelectedTool.TryDownload();
                ViewModel.SelectedTool.TryUpdate();
                if (ViewModel.SelectedTool.isDownloaded)
                {
                    var exePath = System.IO.Path.Combine(toolsDirectoryPath, ViewModel.SelectedTool.ToolExecutablePath);
                    Process proc = new Process();
                    var processStartInfo = proc.StartInfo;
                    processStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                    processStartInfo.FileName = exePath;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    proc.Start();
                    proc.Dispose();
                    Debug.WriteLine($"Launching: {ViewModel.SelectedTool.ToolExecutablePath}");
                }
            }
        }
    }
}
