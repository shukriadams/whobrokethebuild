using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class ParsedBuildLogText
    {
        public IList<ParsedBuildLogTextLine> Items { get; set; }
        
        public string Type { get; set; }
        
        public string Key { get; set; }

        public string Version { get; set; }

        public ParsedBuildLogText()
        {
            this.Type = string.Empty;
            this.Key = string.Empty;
            this.Version = string.Empty;
            this.Items = new List<ParsedBuildLogTextLine>();
        }
    }
}
