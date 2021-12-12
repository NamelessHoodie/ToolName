using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HoodieSuite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
                Debugger.Launch();
#endif
#if RELEASE
            if (!e.Args.Any())
            {
                HoodieShared.Core.CheckUpdatesHoodieSuite(AppDomain.CurrentDomain.BaseDirectory);
            }
#endif
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
