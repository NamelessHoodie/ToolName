﻿using HoodieShared.MVVM.View;
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
        public static void DownloadWithProgressBar(string toolName, string downloadLink, string downloadFilePath)
        {
            if (File.Exists(downloadFilePath))
            {
                File.Delete(downloadFilePath);
            }
            using (var client = new WebClient())
            {
                DownloadNotify dialog = new DownloadNotify($"Please be patient {toolName} is downloading, if your internet connection is slow this process might take a while.");

                client.DownloadProgressChanged += (sender, e) =>
                {
                    var stuff = e as DownloadProgressChangedEventArgs;
                    dialog.Dispatcher.Invoke(() => { dialog.ProgressBar.Value = e.ProgressPercentage; });
                    if (e.ProgressPercentage == 100)
                    {
                        dialog.Dispatcher.Invoke(() => { dialog.Close(); });
                    }
                };
                client.DownloadFileAsync(new Uri(downloadLink), downloadFilePath);
                dialog.ShowDialog();
            }
        }

        public static void HoodieSuiteUpdater(string versionId)
        {
            var hoodieSuitePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            hoodieSuitePath = Directory.GetParent(hoodieSuitePath).FullName;
            hoodieSuitePath = Directory.GetParent(hoodieSuitePath).FullName;
            hoodieSuitePath = Directory.GetParent(hoodieSuitePath).FullName;
            Console.WriteLine(hoodieSuitePath);
            var latest = GithubGetLatestRelease("NamelessHoodie", "HoodieSuite");
            if (latest.Assets.Any())
            {
                if (latest.TagName != versionId)
                {
                    var asset = latest.Assets.First();
                    if (MessageBox.Show($"A new version of HoodieSuite is available = {latest.TagName}.\nThe curent version of HoodieScript is = {versionId}\nWould you like to download and install it?", "HoodieSuite Updater", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string dldFileName = Path.GetFileName(asset.BrowserDownloadUrl);
                        string newFilePath = Path.Combine(hoodieSuitePath, dldFileName);
                        DownloadWithProgressBar($"HoodieSuite - {latest.TagName}", asset.BrowserDownloadUrl, newFilePath);
                        ExtractFile(hoodieSuitePath, newFilePath, hoodieSuitePath);
                    }
                }
            }
            OpenHoodieSuite(hoodieSuitePath);
        }

        public static void ExtractFile(string hoodieScriptBaseDirectoryPath, string sourceArchive, string destination)
        {
            string zPath = Path.Combine(hoodieScriptBaseDirectoryPath, "Tools", "7-Zip_HoodieSuite", "App", "7-Zip", "7zG.exe"); //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
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

        public static void CheckUpdatesHoodieSuite(string hoodieScriptBaseDirectoryPath)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string zPath = Path.Combine(hoodieScriptBaseDirectoryPath, "Tools", "HoodieUpdater", "HoodieUpdater.exe"); //add to proj and set CopyToOuputDir
            var a = GithubGetLatestRelease("NamelessHoodie", "HoodieSuite");
            var b = a.TagName;
            if (assemblyVersion != b)
            {
                try
                {
                    ProcessStartInfo pro = new ProcessStartInfo();
                    pro.WindowStyle = ProcessWindowStyle.Minimized;
                    pro.FileName = zPath;
                    pro.Arguments = assemblyVersion;
                    Process x = Process.Start(pro);
                    System.Windows.Application.Current.Shutdown();
                    //x.WaitForExit();
                }
                catch (System.Exception Ex)
                {
                    Debug.WriteLine(Ex);
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
            return GithubGetLatestRelease(userName, repositoryName).Assets;
        }

        public static Release? GithubGetLatestRelease(string userName, string repositoryName)
        {
            var client = new GitHubClient(new ProductHeaderValue("HoodieUpdater"));
            var user = client.User.Get("NamelessHoodie").GetAwaiter().GetResult();
            var releases = client.Repository.Release.GetAll(userName, repositoryName).GetAwaiter().GetResult();
            if (releases.Any())
            {
                return releases.First();
            }
            return null;
        }
    }
}