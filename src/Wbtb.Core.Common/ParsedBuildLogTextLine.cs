using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class ParsedBuildLogTextLine
    {
        public IList<ParsedBuildLogTextLineItem> Items { get; set; }

        public ParsedBuildLogTextLine()
        {
            this.Items = new List<ParsedBuildLogTextLineItem>();
        }
    }
}
