using HoodieSuite.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HoodieSuite.Properties;
using System.Xml.Linq;
using System.IO;
using System.Net;
using HoodieSuite.MVVM.View;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;
using Octokit;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace HoodieSuite.MVVM.ViewModel
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
                                ToolPath = Path.Combine(exeDirectory, tool.ToolPath),
                            };
                            allTools.Add(tool.ToolArchiveUrl, toolEntry);
                        }

                        if (!gameEntryToolsList.ContainsKey(toolName))
                            gameEntryToolsList.Add(toolName, toolEntry);
                    }
                }
            }
            return gameList;
            throw new NotImplementedException();

            //foreach (var gameXElement in toolDefinitionsXElement.Element("Games").Elements())
            //{
            //    List<string> localGameList = new List<string>();
            //    var gameTypeXAttribute = gameXElement.Attribute("GameType");

            //    if (gameTypeXAttribute == null)
            //    {
            //        throw new Exception($"GameType Attribute Missing from:\n{gameXElement}");
            //    }
            //    else if (string.IsNullOrEmpty(gameTypeXAttribute.Value))
            //    {
            //        throw new Exception($"GameType Attribute is empty in:\n{gameXElement}");
            //    }
            //    if (gameTypeXAttribute.Value.Contains('/'))
            //    {
            //        var a = gameTypeXAttribute.Value.Split('/');
            //        foreach (var gameTitle in a)
            //        {
            //            localGameList.Add(gameTitle);
            //        }
            //    }
            //    else
            //    {
            //        localGameList.Add(gameTypeXAttribute.Value);
            //    }
            //    foreach (var gameTitle in localGameList)
            //    {
            //        var toolsList = new List<ToolEntry>();
            //        foreach (var toolXElement in gameXElement.Elements())
            //        {
            //            var toolEntry = new ToolEntry();
            //            var toolName = toolXElement.Attribute("Name");
            //            if (toolName != null)
            //            {
            //                if (!string.IsNullOrEmpty(toolName.Value))
            //                {
            //                    toolEntry.ToolName = toolName.Value;
            //                }
            //                else
            //                {
            //                    throw new Exception($"ToolName Attribute is empty in:\n{toolXElement}");
            //                }
            //            }
            //            else
            //            {
            //                throw new Exception($"ToolName Attribute Missing from:\n{toolXElement}");
            //            }
            //            var toolDescription = toolXElement.Attribute("Description");
            //            if (toolDescription != null)
            //            {
            //                if (!string.IsNullOrEmpty(toolDescription.Value))
            //                {
            //                    toolEntry.ToolDescription = toolDescription.Value;
            //                }
            //            }
            //            var toolStaticDownloadLink = toolXElement.Attribute("StaticDownloadLink");
            //            if (toolStaticDownloadLink != null)
            //            {
            //                if (!string.IsNullOrEmpty(toolStaticDownloadLink.Value))
            //                {
            //                    var ToolVersion = toolXElement.Attribute("ToolVersion");
            //                    if (ToolVersion != null)
            //                    {
            //                        if (!string.IsNullOrEmpty(ToolVersion.Value))
            //                        {
            //                            var ToolPath = toolXElement.Attribute("ToolPath");
            //                            if (ToolPath != null)
            //                            {
            //                                if (!string.IsNullOrEmpty(ToolPath.Value))
            //                                {
            //                                    string absoluteToolPath = Path.Combine(absoluteToolsFolderPath, ToolPath.Value.Split('\\').First());
            //                                    string absoluteToolPathExe = Path.Combine(absoluteToolsFolderPath, ToolPath.Value);
            //                                    string absoluteVersionFilePath = Path.Combine(absoluteToolPath, $"{toolName.Value.Replace(' ', '_')}.hsvf");
            //                                    toolEntry.ToolPath = absoluteToolPathExe;
            //                                DirectorySetup:
            //                                    if (Directory.Exists(absoluteToolPath))
            //                                    {
            //                                        if (!File.Exists(absoluteVersionFilePath))
            //                                        {
            //                                            File.Create(absoluteVersionFilePath).Dispose();
            //                                        }
            //                                        string versionFileText = File.ReadAllText(absoluteVersionFilePath);
            //                                        if (versionFileText.Trim() != ToolVersion.Value)
            //                                        {
            //                                            Directory.Delete(absoluteToolPath, true);
            //                                            goto DirectorySetup;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        Directory.CreateDirectory(absoluteToolPath);
            //                                        File.Create(absoluteVersionFilePath).Dispose();
            //                                        File.WriteAllText(absoluteVersionFilePath, ToolVersion.Value);
            //                                        string downloadFileName = GetFileNameFromUrl(toolStaticDownloadLink.Value);
            //                                        string downloadFilePath = Path.Combine(absoluteToolPath, downloadFileName);
            //                                        HoodieShared.Core.DownloadWithProgressBar(toolName.Value, toolStaticDownloadLink.Value, downloadFilePath);
            //                                        HoodieShared.Core.ExtractFile(AppDomain.CurrentDomain.BaseDirectory, downloadFilePath, absoluteToolPath);
            //                                        File.Delete(downloadFilePath);
            //                                    }
            //                                }
            //                                else
            //                                {
            //                                    throw new Exception($"ToolPath Attribute is empty in:\n{toolXElement}");
            //                                }
            //                            }
            //                            else
            //                            {
            //                                throw new Exception($"ToolPath Attribute Missing from:\n{toolXElement}");
            //                            }
            //                        }
            //                        else
            //                        {
            //                            throw new Exception($"ToolVersion Attribute is empty in:\n{toolXElement}");
            //                        }
            //                    }
            //                    else
            //                    {
            //                        throw new Exception($"ToolVersion Attribute Missing from:\n{toolXElement}");
            //                    }
            //                }
            //            }
            //            //toolsList.Add(toolEntry);
            //            if (!toolsDictionary.ContainsKey(gameTitle))
            //            {
            //                toolsDictionary.Add(gameTitle, toolsList);
            //            }
            //            else
            //            {
            //                if (!toolsDictionary[gameTitle].Contains(toolEntry))
            //                    toolsDictionary[gameTitle].Add(toolEntry);
            //            }
            //        }
            //    }

            //    //toolsDictionary.Add(gameTypeXAttribute.Value, toolsList);
            //}
            //return toolsDictionary;

            string GetFileNameFromUrl(string url)
            {
                Uri SomeBaseUri = new Uri("http://canbeanything");
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                    uri = new Uri(SomeBaseUri, url);

                return Path.GetFileName(uri.LocalPath);
            }
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
            else if(isUpdate)
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
