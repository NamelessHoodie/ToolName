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
                                LatestToolVersion = tool.ToolVersion,
                                ToolExecutablePath = tool.ToolPath,
                            };
                            allTools.Add(tool.ToolArchiveUrl, toolEntry);
                        }

                        if (!gameEntryToolsList.ContainsKey(toolName))
                            gameEntryToolsList.Add(toolName, toolEntry);
                    }
                }
            }

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
                    text.Append($"{tool.ToolName} - {tool.LatestToolVersion},\n");
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
                        tool.TryDownload(false);
                    }
                    return true;
                }
            }
            return false;
        }

        private static void AskUserIfUpdateAll(string toolsFolderPath, IEnumerable<ToolEntry> toolslist)
        {
            var updatesAvalaible = ToolsHaveUpdates(toolsFolderPath, toolslist);
            if (updatesAvalaible.Any())
            {
                StringBuilder text = new StringBuilder();
                string label;

                text.Append($"{(updatesAvalaible.Count() > 1 ? updatesAvalaible.Count() : "")} {(updatesAvalaible.Count() > 1 ? "Updates" : "An update")} {(updatesAvalaible.Count() > 1 ? "are" : "is")} available. Would you like to download {(updatesAvalaible.Count() > 1 ? "them" : "it")}?\n\n");
                text.Append($"{(updatesAvalaible.Count() > 1 ? "Updates" : "Update")} Found For:\n");
                for (int i = 0; i < updatesAvalaible.Count(); i++)
                {
                    var tool = updatesAvalaible.ElementAt(i);
                    text.Append($"{tool.ToolName} - {tool.CurrentToolVersion} => {tool.LatestToolVersion},\n");
                    if (i == (updatesAvalaible.Count() - 1))
                    {
                        text.Length -= 2;
                    }
                }
                text.AppendLine();

                label = $"{(updatesAvalaible.Count() > 1 ? "Updates" : "Update")} found!";
                if (MessageBox.Show(text.ToString(), label, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var tool in updatesAvalaible)
                    {
                        tool.TryUpdate(false);
                    }
                }
            }
        }


        private static IEnumerable<ToolEntry> ToolsHaveUpdates(string toolsFolderPath, IEnumerable<ToolEntry> toolEntries)
        {
            var a = new List<ToolEntry>();
            foreach (var tool in toolEntries)
            {
                if(tool.hasUpdates)
                {
                    a.Add(tool);
                }
            }
            return a;
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
