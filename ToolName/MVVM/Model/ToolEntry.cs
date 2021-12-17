using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace ToolName.MVVM.Model
{
    public class ToolEntry : ObservableObject
    {
        private string GetNumbers(string text)
        {
            text = text ?? string.Empty;
            return new string(text.Where(p => char.IsDigit(p)).ToArray());
        }

        public string IconPath
        {
            get 
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons"));
                var iconsMatch = dir.GetFiles(ToolName + ".*");
                if (iconsMatch.Any())
                {
                    return iconsMatch.First().FullName;
                }
                else
                {
                    FileInfo[] defaultIcons = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "DefaultIcons")).GetFiles();
                    byte[] hashedToolName = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(ToolName));
                    int hashIntSum = hashedToolName.Sum(item => item);
                    int indexFromHash = new Random(hashIntSum).Next(0, defaultIcons.Count() - 1);
                    if (defaultIcons.Count() >= 1)
                    {
                        return defaultIcons[indexFromHash].FullName;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        public string ToolName { get; set; }
        public string ToolDescription { get; set; }
        public string ToolDownloadUrl { get; set; }
        public string ToolPath { get; set; }
        public string ToolVersion { get; set; }
        public bool isDownloaded 
        {
            get 
            {
                return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools" ,ToolPath));
            }
        }
        public void NotifyDownloadedStateChanged()
        {
            NotifyPropertyChanged(nameof(isDownloaded));
        }
        public UserControl ToolView { get; set; }
        public ToolEntry() 
        {
            
        }
    }
}
