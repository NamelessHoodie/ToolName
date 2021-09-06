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

namespace HoodieSuite.MVVM.View
{
    /// <summary>
    /// Interaction logic for DownloadNotify.xaml
    /// </summary>
    public partial class DownloadNotify : Window
    {
        public DownloadNotify()
        {
            InitializeComponent();
        }

        public DownloadNotify(string title)
        {
            InitializeComponent();
            TitleText = title;
        }

        public string TitleText
        {
            get { return TitleTextBox.Text; }
            set { TitleTextBox.Text = value; }
        }

        public bool Canceled { get; set; }

        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Canceled = false;
            Close();
        }
    }
}
