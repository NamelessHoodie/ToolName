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
using System.Diagnostics;

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
            int[] arr = new int[] {1,2,3,4,5,6,7,8,9,10 };
            foreach (var item in Enumerable.Range(0, arr.Length).Reverse())
            {
                Debug.WriteLine(arr[item]);
            } 
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TestListBox.Items.Clear();
                BND4 anibnd = BND4.Read(ofd.FileName);
                TAE3 tae = TAE3.Read(anibnd.Files.First(item => item.ID == 5000000 + 4).Bytes);
                foreach (var item in tae.Animations)
                {
                    TestListBox.Items.Add(item.ID);
                }
            }
        }
    }
}
