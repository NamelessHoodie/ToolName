using HoodieShared.MVVM.View;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HoodieShared
{
    public class Core
    {
#if DEBUG
public static string buildType = "-debug";
#endif
#if RELEASE
        public static string buildType = "-stable";
#endif
        public static void DownloadWithProgressBar(string toolName, string downloadLink, string downloadFilePath)
        {
            if (File.Exists(downloadFilePath))
            {
                File.Delete(downloadFilePath);
            }
            using (var client = new WebClient())
            {
                DownloadNotify dialog = new DownloadNotify($"Please be patient {toolName} is downloading, if your internet connection is slow this process might take a while.");

                client.DownloadFileCompleted += (sender, e) =>
                {
                    dialog.Dispatcher.Invoke(() => { dialog.Close(); });
                    if (e.Error != null)
                        MessageBox.Show(e.Error.Message);
                };
                client.DownloadProgressChanged += (sender, e) =>
                {
                    var stuff = e as DownloadProgressChangedEventArgs;
                    dialog.Dispatcher.Invoke(() => { dialog.ProgressBar.Value = e.ProgressPercentage; });
                };
                client.DownloadFileAsync(new Uri(downloadLink), downloadFilePath);
                dialog.ShowDialog();
            }
        }

        public static void ToolNameUpdater(string versionId, string newVersionId)
        {
            var toolNamePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            toolNamePath = Directory.GetParent(toolNamePath).FullName;
            toolNamePath = Directory.GetParent(toolNamePath).FullName;
            toolNamePath = Directory.GetParent(toolNamePath).FullName;
            var latest = GithubGetReleases("NamelessHoodie", "ToolName").First(x => x.TagName == newVersionId);
            if (latest.Assets.Any())
            {
                if (latest.TagName != versionId)
                {
                    var asset = latest.Assets.First();
                    if (MessageBox.Show($"A new version of ?ToolName? is available = {newVersionId}.\nThe curent version of ?ToolName? is = {versionId}\nWould you like to download and install it?", "?ToolName? Updater", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string dldFileName = Path.GetFileName(asset.BrowserDownloadUrl);
                        string newFilePath = Path.Combine(toolNamePath, dldFileName);
                        DownloadWithProgressBar($"ToolName - {latest.TagName}", asset.BrowserDownloadUrl, newFilePath);
                        ExtractFile(toolNamePath, newFilePath, toolNamePath);
                    }
                }
            }
            OpenToolName(toolNamePath);
        }

        public static void ExtractFile(string toolNameBaseDirectoryPath, string sourceArchive, string destination)
        {
            string zPath = Path.Combine(toolNameBaseDirectoryPath, "Tools", "7-Zip_ToolNameBundled", "App", "7-Zip", "7z.exe"); //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.CreateNoWindow = true;
                pro.FileName = zPath;
                //Debug.WriteLine(string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination));
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination);
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception Ex)
            {
                Debug.WriteLine(Ex);
            }
        }

        public static void CheckUpdatesToolName(string toolNameBaseDirectoryPath)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string zPath = Path.Combine(toolNameBaseDirectoryPath, "Tools", "HoodieUpdater", "HoodieUpdater.exe"); //add to proj and set CopyToOuputDir
            string toolNamePath = Path.Combine(toolNameBaseDirectoryPath, "ToolName.exe");
            var latestRelease = GithubGetLatestRelease("NamelessHoodie", "ToolName");
            if (latestRelease != null)
            {
                var assemblyVersionFormatted = $"{assemblyVersion}{buildType}";
                if (assemblyVersionFormatted != latestRelease.TagName)
                {
                    try
                    {
                        ProcessStartInfo pro = new ProcessStartInfo();
                        pro.WindowStyle = ProcessWindowStyle.Normal;
                        pro.WorkingDirectory = Path.GetDirectoryName(zPath);
                        pro.FileName = zPath;
                        pro.ArgumentList.Add(assemblyVersionFormatted);
                        pro.ArgumentList.Add(latestRelease.TagName);
                        foreach (var process in Process.GetProcessesByName("ToolName"))
                        {
                            if (process != Process.GetCurrentProcess())
                            {
                                if (process.MainModule.FileName != toolNamePath)
                                {
                                    process.Kill();
                                    process.WaitForExit();
                                }
                            }
                        }
                        Process x = Process.Start(pro);
                        Environment.Exit(0);
                        x.WaitForExit();
                    }
                    catch (System.Exception Ex)
                    {
                        Debug.WriteLine(Ex);
                    }
                }
            }
        }

        public static void OpenToolName(string toolNameBaseDirectoryPath)
        {
            string zPath = Path.Combine(toolNameBaseDirectoryPath, "ToolName.exe"); //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Normal;
                pro.WorkingDirectory = Path.GetDirectoryName(zPath);
                pro.FileName = zPath;
                pro.Arguments = "true";
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception Ex)
            {
                Debug.WriteLine(Ex);
            }
        }

        public static IReadOnlyList<ReleaseAsset> GithubGetLatestReleaseAssets(string userName, string repositoryName)
        {
            return GithubGetLatestRelease(userName, repositoryName).Assets;
        }

        public static Release? GithubGetLatestRelease(string userName, string repositoryName)
        {
            IEnumerable<Release> releases = GithubGetReleases(userName, repositoryName);
            if (releases.Any())
            {
                return releases.First();
            }
            return null;
        }

        private static IEnumerable<Release> GithubGetReleases(string userName, string repositoryName)
        {
            var client = new GitHubClient(new ProductHeaderValue("HoodieUpdater"));
            var user = client.User.Get("NamelessHoodie").GetAwaiter().GetResult();
            var releases = client.Repository.Release.GetAll(userName, repositoryName).GetAwaiter().GetResult().Where(x => x.TagName.EndsWith(buildType));
            return releases;
        }
    }
}
