using HoodieSuite.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HoodieSuite.Properties;

namespace HoodieSuite.MVVM.ViewModel
{
    public class DashboardViewModel
    {

        public Dictionary<string, List<ToolEntry>> GameToolPage { get; } = new Dictionary<string, List<ToolEntry>>() 
        { 
            { "Dark Souls - PTDE", new List<ToolEntry>()
                {
                    new ToolEntry() { ToolName="MemeTool0"}, 
                    new ToolEntry() { ToolName="MemeTool1"} 
                } 
            },
            { "Dark Souls - Remastered", new List<ToolEntry>()
                {
                    new ToolEntry() { ToolName="RemasteredTool0"},
                    new ToolEntry() { ToolName="RemasteredTool1"},
                    new ToolEntry() { ToolName="RemasteredTool2"}
                }
            }
        };
        public List<String> SupportedGames { get; set; }
        public String SelectedGame { get; set; }
        public DashboardViewModel()
        {
            SupportedGames = GameToolPage.Keys.ToList();
        }
    }
}
