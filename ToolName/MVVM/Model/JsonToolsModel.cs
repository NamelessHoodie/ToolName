using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolName.MVVM.Model
{
    public class JsonToolsModel
    {
        public List<string> SupportedGames { get; set; } = new List<string>();
        public Dictionary<string, JsonToolModel> Tools { get; set; } = new Dictionary<string, JsonToolModel>();
        public JsonToolsModel()
        {
            
        }
    }
}
