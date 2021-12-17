using ToolName.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ToolName.Properties;
using System.Xml.Linq;
using System.IO;
using System.Net;
using ToolName.MVVM.View;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;
using Octokit;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ToolName.MVVM.ViewModel
{
    public class DashboardViewModel
    {

        public Dictionary<string, Dictionary<string, ToolEntry>> GameToolPage { get; }

        public static Dictionary<string, Dictionary<string, ToolEntry>> ToolDefParserAndDownloader()
        {


            //Get Tools Folder Path
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string absoluteToolsFolderPath = Path.Combine(exeDirectory, "Tools");

            //Initialize Serializer 
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.Formatting = Formatting.Indented;

            //Create list to deserialize the json into
            List<JsonToolsModel> listJson;

            //Deserialize json
            using (TextReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Definitions", "ToolsDef.json")))
            using (var jsonRreader = new Newtonsoft.Json.JsonTextReader(reader))
            {
                listJson = serializer.Deserialize<List<JsonToolsModel>>(jsonRreader);
            }

            //Populate Dictionary of Dictionaries of ToolEntry from Deserialized Json
            Dictionary<string, Dictionary<string, ToolEntry>> gameList = new Dictionary<string, Dictionary<string, ToolEntry>>();

            Dictionary<string, ToolEntry> allTools = new Dictionary<string, ToolEntry>();

            //Tool Lists
            foreach (var toolsModel in listJson)
            {
                //Supported Games in the curent Tool List
                foreach (var game in toolsModel.SupportedGames)
                {

                    //If the game does not have a Dictionary of ToolEntries yet, initialize it
                    if (!gameList.ContainsKey(game))
                        gameList.Add(game, new Dictionary<string, ToolEntry>());

                    var gameEntryToolsList = gameList[game];
                    foreach (var (toolName, tool) in toolsModel.Tools)
                    {
                        ToolEntry toolEntry;

                        //Check if the all tools Dictionary already contains an instance of that tool and assigns it to toolEntry
                        if (allTools.ContainsKey(tool.ToolArchiveUrl))
                            toolEntry = allTools[tool.ToolArchiveUrl];
                        else
                        {
                            toolEntry = new ToolEntry()
                            {
                                ToolName = toolName,
                                ToolDescription = tool.ToolDescription,
                                ToolDownloadUrl = tool.ToolArchiveUrl,
                                ToolVersion = tool.ToolVersion,
                                ToolPath = tool.ToolPath,
                            };
                            allTools.Add(tool.ToolArchiveUrl, toolEntry);
                        }

                        if (!gameEntryToolsList.ContainsKey(toolName))
                            gameEntryToolsList.Add(toolName, toolEntry);
                    }
                }
            }

            //foreach (var (toolName, toolEntry) in allTools)
            //{
            //    TryDownloadTool(absoluteToolsFolderPath, toolEntry, false);
            //    TryUpdateTool(absoluteToolsFolderPath, toolEntry, false);
            //}
            var toolsList = allTools.Values;
            var isAskDownloadAllTools = bool.Parse(ResourcesRW.ReadKeyFromResourceFile("isAskDownloadAllTools", true.ToString()));
            if (isAskDownloadAllTools)
            {
                AskUserIfDownloadAll(absoluteToolsFolderPath, toolsList);
                ResourcesRW.AddOrUpdateResource("isAskDownloadAllTools", false.ToString());
            }
            AskUserIfUpdateAll(absoluteToolsFolderPath, toolsList);

            return gameList;
        }

        static string GetFileNameFromUrl(string url)
        {
            Uri SomeBaseUri = new Uri("http://canbeanything");
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return Path.GetFileName(uri.LocalPath);
        }

        private static bool AskUserIfDownload(ToolEntry tool, string oldVersionStr = null)
        {
            string text;
            string label;

            if (oldVersionStr != null)
            {
                text = $"New version of {tool.ToolName} found {tool.ToolVersion}\nWould you like to download it? The current Version is {oldVersionStr}.";
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

        private static bool AskUserIfDownloadAll(string toolsFolderPath, IEnumerable<ToolEntry> toolslist)
        {
            var updatesAvalaible = toolslist.Where(x => !x.isDownloaded);
            if (updatesAvalaible.Any())
            {
                StringBuilder text = new StringBuilder();
                string label;

                text.Append($"Would you like to download all tools?\nThis message will only be displayed once, choose Carefully.\n\nAvailable tools are:\n");
                for (int i = 0; i < updatesAvalaible.Count(); i++)
                {
                    var tool = updatesAvalaible.ElementAt(i);
                    text.Append($"{tool.ToolName} - {tool.ToolVersion},\n");
                    if (i == (updatesAvalaible.Count() - 1))
                    {
                        text.Length -= 2;
                    }
                }
                text.AppendLine();

                label = $"Would you like to download all tools?";
                if (MessageBox.Show(text.ToString(), label, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var tool in updatesAvalaible)
                    {
                        TryDownloadTool(toolsFolderPath, tool, false);
                    }
                    return true;
                }
            }
            return false;
        }

        private static void AskUserIfUpdateAll(string toolsFolderPath, IEnumerable<ToolEntry> toolslist)
        {
            var updatesAvalaible = ToolsHaveUpdates(toolsFolderPath, toolslist).Where(x => x.found);
            if (updatesAvalaible.Any())
            {
                StringBuilder text = new StringBuilder();
                string label;

                text.Append($"{(updatesAvalaible.Count() > 1 ? updatesAvalaible.Count() : "")} {(updatesAvalaible.Count() > 1 ? "Updates" : "An update")} {(updatesAvalaible.Count() > 1 ? "are" : "is")} available. Would you like to download {(updatesAvalaible.Count() > 1 ? "them" : "it")}?\n\n");
                text.Append($"{(updatesAvalaible.Count() > 1 ? "Updates" : "Update")} Found For:\n");
                for (int i = 0; i < updatesAvalaible.Count(); i++)
                {
                    var (oldVersion, newVersion, tool, found) = updatesAvalaible.ElementAt(i);
                    text.Append($"{tool.ToolName} - {oldVersion} => {newVersion},\n");
                    if (i == (updatesAvalaible.Count() - 1))
                    {
                        text.Length -= 2;
                    }
                }
                text.AppendLine();

                label = $"{(updatesAvalaible.Count() > 1 ? "Updates" : "Update")} found!";
                if (MessageBox.Show(text.ToString(), label, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var (oldVersion, newVersion, tool, found) in updatesAvalaible)
                    {
                        TryUpdateTool(toolsFolderPath, tool, false);
                    }
                }
            }
        }

        public static bool TryDownloadTool(string toolsFolderPath, ToolEntry tool, bool isUserPromptDl = true)
        {
            string absoluteToolPath = Path.Combine(toolsFolderPath, tool.ToolPath.Split('\\').First());
            string absoluteVersionFilePath = Path.Combine(absoluteToolPath, $"{tool.ToolName.Replace(' ', '_')}.hsvf");
            if (!tool.isDownloaded)
            {
                if (isUserPromptDl ? AskUserIfDownload(tool) : true)
                {
                    WriteToolToDisk(tool, absoluteToolPath, absoluteVersionFilePath);
                    return true;
                }
            }
            return false;
            //return TryUpdateTool(toolsFolderPath, tool, isUserPromptDl);
        }

        private static void WriteToolToDisk(ToolEntry tool, string absoluteToolPath, string absoluteVersionFilePath)
        {
            Directory.CreateDirectory(absoluteToolPath);
            if (!File.Exists(absoluteVersionFilePath))
                File.Create(absoluteVersionFilePath).Dispose();
            File.WriteAllText(absoluteVersionFilePath, tool.ToolVersion);
            string downloadFileName = GetFileNameFromUrl(tool.ToolDownloadUrl);
            string downloadFilePath = Path.Combine(absoluteToolPath, downloadFileName);
            HoodieShared.Core.DownloadWithProgressBar(tool.ToolName, tool.ToolDownloadUrl, downloadFilePath);
            HoodieShared.Core.ExtractFile(AppDomain.CurrentDomain.BaseDirectory, downloadFilePath, absoluteToolPath);
            File.Delete(downloadFilePath);
            tool.NotifyDownloadedStateChanged();
        }

        private static IEnumerable<(string oldVersion, string newVersion, ToolEntry tool, bool found)> ToolsHaveUpdates(string toolsFolderPath, IEnumerable<ToolEntry> toolEntries)
        {
            var a = new List<(string oldVersion, string newVersion, ToolEntry tool, bool found)>();
            foreach (var tool in toolEntries)
            {
                var help = IsToolUpdatesAvailable(toolsFolderPath, tool);
                if (help.found)
                {
                    a.Add((help.oldVersion, help.newVersion, tool, help.found));
                }
            }
            return a;
        }

        public static (string oldVersion, string newVersion, bool found) IsToolUpdatesAvailable(string toolsFolderPath, ToolEntry tool)
        {
            string toolFolderPath = Path.Combine(toolsFolderPath, tool.ToolPath.Split('\\').First());
            string toolExePathAbsolute = Path.Combine(toolsFolderPath, tool.ToolPath);
            string versionFilePath = Path.Combine(toolFolderPath, $"{tool.ToolName.Replace(' ', '_')}.hsvf");
            if (Directory.Exists(toolsFolderPath))
            {
                if (File.Exists(versionFilePath))
                {
                    string versionFileText = File.ReadAllText(versionFilePath).Trim();
                    if (versionFileText != tool.ToolVersion)
                    {
                        return (versionFileText, tool.ToolVersion, true);
                    }
                }
            }
            return (String.Empty, String.Empty, false);
        }

        public static bool TryUpdateTool(string toolsFolderPath, ToolEntry tool, bool isUserPromptDl = true)
        {
            string toolFolderPath = new DirectoryInfo(Path.Combine(toolsFolderPath, tool.ToolPath.Split('\\').First())).FullName;
            string toolExePathAbsolute = Path.Combine(toolsFolderPath, tool.ToolPath);
            string versionFilePath = Path.Combine(toolFolderPath, $"{tool.ToolName.Replace(' ', '_')}.hsvf");
            var hi = IsToolUpdatesAvailable(toolsFolderPath, tool);
            if (hi.found)
            {
                string toolFolderPathAfterRename = toolFolderPath + "-RenamedForUpdating";
                bool pressedRetry = false;
            ComeBackLol:
                try
                {
                    if (pressedRetry ? true : isUserPromptDl ? AskUserIfDownload(tool, hi.oldVersion) : true)
                    {
                        Directory.Move(toolFolderPath, toolFolderPathAfterRename);
                        Directory.Delete(toolFolderPathAfterRename, true);
                        WriteToolToDisk(tool, toolFolderPath, versionFilePath);
                        return true;
                    }
                }
                catch (Exception)
                {
                    if (MessageBox.Show($"WARNING: Could not update {tool.ToolName}.\nVersion {hi.oldVersion} => {tool.ToolVersion}\n{tool.ToolPath} was already being accessed.", $"WARNING: Could not update { tool.ToolName}", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    {
                        pressedRetry = true;
                        goto ComeBackLol;
                    }
                }
            }
            return false;
        }

        public List<string> SupportedGames { get; set; }
        public string SelectedGame { get; set; }
        public ToolEntry SelectedTool { get; set; }
        public void HoodieUpdater_Update()
        {
            bool isUpdate = true;
            string updaterFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HoodieUpdater");
            if (!Directory.Exists(updaterFolderPath))
            {
                Directory.CreateDirectory(updaterFolderPath);
            }
            else if (isUpdate)
            {
                Directory.Delete(updaterFolderPath, true);
                Directory.CreateDirectory(updaterFolderPath);
            }
        }
        public DashboardViewModel()
        {
            GameToolPage = ToolDefParserAndDownloader();
            SupportedGames = GameToolPage.Keys.ToList();
        }
    }
}
