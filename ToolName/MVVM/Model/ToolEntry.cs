using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ToolName.MVVM.Model
{
    public class ToolEntry : ObservableObject
    {
        private string GetNumbers(string text)
        {
            text = text ?? string.Empty;
            return new string(text.Where(p => char.IsDigit(p)).ToArray());
        }

        private static bool AskUserIfDownload(ToolEntry tool, string oldVersionStr = null)
        {
            string text;
            string label;

            if (oldVersionStr != null)
            {
                text = $"New version of {tool.ToolName} found {tool.LatestToolVersion}\nWould you like to download it? The current Version is {oldVersionStr}.";
                label = $"Update found!";
            }
            else
            {
                text = $"{tool.ToolName} is not installed\nWould you like to download it?";
                label = $"{tool.ToolName} not found.";
            }
            if (MessageBox.Show(text, label, MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            return false;
        }

        static string GetFileNameFromUrl(string url)
        {
            Uri SomeBaseUri = new Uri("http://canbeanything");
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return Path.GetFileName(uri.LocalPath);
        }

        private static void WriteToolToDisk(ToolEntry tool, string absoluteToolPath, string absoluteVersionFilePath)
        {
            if (Directory.Exists(absoluteToolPath))
            {
                Directory.Delete(absoluteToolPath, true);
            }
            Directory.CreateDirectory(absoluteToolPath);
            if (!File.Exists(absoluteVersionFilePath))
                File.Create(absoluteVersionFilePath).Dispose();
            File.WriteAllText(absoluteVersionFilePath, tool.LatestToolVersion);
            string downloadFileName = GetFileNameFromUrl(tool.ToolDownloadUrl);
            string downloadFilePath = Path.Combine(absoluteToolPath, downloadFileName);
            HoodieShared.Core.DownloadWithProgressBar(tool.ToolName, tool.ToolDownloadUrl, downloadFilePath);
            HoodieShared.Core.ExtractFile(AppDomain.CurrentDomain.BaseDirectory, downloadFilePath, absoluteToolPath);
            File.Delete(downloadFilePath);
            tool.NotifyDownloadedStateChanged();
        }

        public bool TryUpdate(bool isUserPromptDl = true)
        {
            string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
            string toolFolderPath = new DirectoryInfo(Path.Combine(toolsFolderPath, ToolFolderPath)).FullName;
            string versionFilePath = Path.Combine(toolFolderPath, $"{ToolName.Replace(' ', '_')}.hsvf");
            if (hasUpdates)
            {
                string toolFolderPathAfterRename = toolFolderPath + "-RenamedForUpdating";
                bool pressedRetry = false;
            Retry:
                try
                {
                    if (pressedRetry ? true : isUserPromptDl ? AskUserIfDownload(this, CurrentToolVersion) : true)
                    {
                        Directory.Move(toolFolderPath, toolFolderPathAfterRename);
                        Directory.Delete(toolFolderPathAfterRename, true);
                        WriteToolToDisk(this, toolFolderPath, versionFilePath);
                        return true;
                    }
                }
                catch (Exception)
                {
                    if (MessageBox.Show($"WARNING: Could not update {ToolName}, because it was already being accessed.\nWould you like to try closing it and attempt to update again?", $"WARNING: Could not update {ToolName}", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    {
                        pressedRetry = true;
                        CloseAnyInstancesOfTool();
                        goto Retry;
                    }
                }
            }
            return false;
        }

        public bool TryDownload(bool isUserPromptDl = true)
        {
            string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
            string absoluteToolPath = Path.Combine(toolsFolderPath, ToolFolderPath);
            string absoluteVersionFilePath = Path.Combine(absoluteToolPath, $"{ToolName.Replace(' ', '_')}.hsvf");
            if (!isDownloaded)
            {
                bool pressedRetry = false;
                Retry:
                try
                {
                    if (pressedRetry ? true : isUserPromptDl ? AskUserIfDownload(this, CurrentToolVersion) : true)
                    {
                        WriteToolToDisk(this, absoluteToolPath, absoluteVersionFilePath);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (pressedRetry)
                    {
                        return false;
                    }
                    pressedRetry = true;
                    CloseAnyInstancesOfTool();
                    goto Retry;
                }
            }
            return false;
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
        public string ToolFolderPath
        {
            get 
            {
                return ToolExecutablePath.Split('\\').First();
            }
        }
        public string ToolExecutablePath { get; set; }
        public string LatestToolVersion { get; set; }
        public string? CurrentToolVersion
        {
            get
            {
                string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                string toolExePath = Path.Combine(toolsFolderPath, ToolExecutablePath.Split('\\').First());
                string versionFilePath = Path.Combine(toolExePath, $"{ToolName.Replace(' ', '_')}.hsvf");
                if (isDownloaded)
                {
                    string versionFileText = File.ReadAllText(versionFilePath).Trim();
                    return versionFileText;
                }
                return null;
            }
        }
        public bool hasUpdates
        {
            get
            {
                var toolVer = CurrentToolVersion;
                return toolVer != null && toolVer != LatestToolVersion;
            }
        }
        public bool isDownloaded
        {
            get
            {
                string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                string toolFolderPath = Path.Combine(toolsFolderPath, ToolFolderPath);
                string toolExePath = Path.Combine(toolsFolderPath, ToolExecutablePath);
                string versionFilePath = Path.Combine(toolFolderPath, $"{ToolName.Replace(' ', '_')}.hsvf");
                return File.Exists(toolExePath) && File.Exists(versionFilePath);
            }
            set 
            {
                if (!value)
                {
                    if (isDownloaded)
                    {
                        if (MessageBox.Show($"Are you sure you want to uninstall {ToolName}?", "Uninstall prompt", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                            string toolFolderPath = Path.Combine(toolsFolderPath, ToolFolderPath);
                        Retry:
                            try
                            {
                                Directory.Delete(toolFolderPath, true);
                            }
                            catch (Exception)
                            {

                                if (MessageBox.Show($"Could not remove all files from {ToolName} access denied.\nWould you like to try closing {ToolName} and retry again?", "Uninstall failed.", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    CloseAnyInstancesOfTool();
                                }
                            }
                        }
                    }
                }
                else
                {
                    TryDownload();
                }
            }
        }

        private void CloseAnyInstancesOfTool()
        {
            string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
            var a = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ToolExecutablePath));
            foreach (var process in a)
            {
                if (process.MainModule.FileName == Path.Combine(toolsFolderPath, ToolExecutablePath))
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
        }

        public void CloseToolAllInstances() 
        {
            
        }
        public void NotifyDownloadedStateChanged()
        {
            NotifyPropertyChanged(nameof(isDownloaded));
            NotifyPropertyChanged(nameof(hasUpdates));
            NotifyPropertyChanged(nameof(CurrentToolVersion));
            NotifyPropertyChanged(nameof(IconPath));
        }
        public System.Windows.Controls.UserControl ToolView { get; set; }
        public ToolEntry()
        {

        }
    }
}
