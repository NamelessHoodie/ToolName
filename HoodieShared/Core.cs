using HoodieShared.MVVM.View;
using Octokit;
using SevenZipExtractor;
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
        public static string buildType = "-debug";
#endif
        public static bool DownloadWithProgressBar(string toolName, string downloadLink, string downloadFilePath)
        {
            bool success = false;
            if (File.Exists(downloadFilePath))
            {
                File.Delete(downloadFilePath);
            }
            using (var client = new WebClient())
            {
                DownloadNotify dialog = new DownloadNotify($"Please be patient {toolName} is downloading, if your internet connection is slow this process might take a while.");
                client.DownloadFileCompleted += (sender, e) =>
                {
                    if (e.Error == null)
                        success = true;
                    dialog.Dispatcher.Invoke(() => { dialog.Close(); });
                };
                client.DownloadProgressChanged += (sender, e) =>
                {
                    var stuff = e as DownloadProgressChangedEventArgs;
                    dialog.Dispatcher.Invoke(() => { dialog.ProgressBar.Value = e.ProgressPercentage; });
                };
                client.DownloadFileAsync(new Uri(downloadLink), downloadFilePath);
                dialog.ShowDialog();
            }
            return success;
        }

        public static void HoodieSuiteUpdater(string versionId, string releaseVersion)
        {
            var hoodieSuitePath = Path.Combine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), @"..\", @"..\");
            var latest = GithubGetReleasesStable("NamelessHoodie", "HoodieSuite").Where(x => x.TagName == releaseVersion).First();
            var asset = latest.Assets.First();
            if (MessageBox.Show($"A new version of HoodieSuite is available = {latest.TagName}.\nThe curent version of HoodieScript is = {versionId}\nWould you like to download and install it?", "HoodieSuite Updater", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string dldFileName = Path.GetFileName(asset.BrowserDownloadUrl);
                string newFilePath = Path.Combine(hoodieSuitePath, @"..\", dldFileName);
                if (DownloadWithProgressBar($"HoodieSuite - {latest.TagName}", asset.BrowserDownloadUrl, newFilePath))
                {
                    //Directory.Delete(hoodieSuitePath, true);
                    ExtractFile(hoodieSuitePath, newFilePath, hoodieSuitePath);
                    File.Delete(newFilePath);
                }
            }
            OpenHoodieSuite(hoodieSuitePath);
        }

        public static bool ExtractFile(string hoodieScriptBaseDirectoryPath, string sourceArchive, string destination)
        {
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(sourceArchive))
                {
                    archiveFile.Extract(delegate (Entry entry)
                    {
                        string text = Path.Combine(destination, entry.FileName);
                        if (entry.IsFolder && !text.Contains("HoodieUpdater"))
                        {
                            return text;
                        }

                        return (!File.Exists(text) || true) ? text : null;
                    });
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public static void CheckUpdatesHoodieSuite(string hoodieScriptBaseDirectoryPath)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string zPath = Path.Combine(hoodieScriptBaseDirectoryPath, "Tools", "HoodieUpdater", "HoodieUpdater.exe"); //add to proj and set CopyToOuputDir
            var a = GithubGetLatestReleaseStable("NamelessHoodie", "HoodieSuite");
            if (a != null)
            {
                var targetVersion = a.TagName;
                string assemblyVersionFormatted = $"{assemblyVersion}{buildType}";
                if (assemblyVersionFormatted != targetVersion)
                {
                    try
                    {
                        ProcessStartInfo pro = new ProcessStartInfo();
                        pro.WindowStyle = ProcessWindowStyle.Minimized;
                        pro.FileName = zPath;
                        pro.ArgumentList.Add(assemblyVersionFormatted);
                        pro.ArgumentList.Add(targetVersion);
                        Process x = Process.Start(pro);
                        System.Windows.Application.Current.Shutdown();
                        x.WaitForExit();
                    }
                    catch (System.Exception Ex)
                    {
                        Debug.WriteLine(Ex);
                    }
                }
            }
        }

        public static void OpenHoodieSuite(string hoodieScriptBaseDirectoryPath)
        {
            string zPath = Path.Combine(hoodieScriptBaseDirectoryPath, "HoodieSuite.exe"); //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Minimized;
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
            return GithubGetLatestReleaseStable(userName, repositoryName).Assets;
        }

        public static Release? GithubGetLatestReleaseStable(string userName, string repositoryName)
        {
            IEnumerable<Release> releases = GithubGetReleasesStable(userName, repositoryName);
            if (releases.Any())
            {
                return releases.First();
            }
            return null;
        }

        private static IEnumerable<Release> GithubGetReleasesStable(string userName, string repositoryName)
        {
            var client = new GitHubClient(new ProductHeaderValue("HoodieUpdater"));
            var user = client.User.Get("NamelessHoodie").GetAwaiter().GetResult();
            var releases = client.Repository.Release.GetAll(userName, repositoryName).GetAwaiter().GetResult().Where(x => x.TagName.EndsWith(buildType));
            return releases;
        }
    }
}
