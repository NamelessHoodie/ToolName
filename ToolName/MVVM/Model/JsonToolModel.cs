using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolName.MVVM.Model
{
    public class JsonToolModel
    {
        public string ToolDescription { get; set; }
        public string ToolArchiveUrl { get; set; }
        public string ToolPath { get; set; }
        public string ToolVersion { get; set; }
        public JsonToolModel() 
        { 
        }
    }
}
