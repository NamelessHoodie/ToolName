using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using SoulsFormats;

namespace HoodieSuite.MVVM.View
{
    /// <summary>
    /// Interaction logic for SoulsFormatsTester.xaml
    /// </summary>
    public partial class SoulsFormatsTester : Window
    {
        public SoulsFormatsTester()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < TestListBox.Items.Count; i++)
                {
                    TestListBox.Items.Remove(0);
                }
                BND4 anibnd = BND4.Read(ofd.FileName);
                TAE3 tae = TAE3.Read(anibnd.Files.First(item => item.ID == 5000000 + 627).Bytes);
                foreach (var item in tae.Animations)
                {
                    TestListBox.Items.Add(item.ID);
                }
            }
        }
    }
}
