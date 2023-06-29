using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wbtb.Core.Common
{
    public class ParsedBuildLogTextLineItem
    {
        public string Content { get; set; }
        public string Type { get; set; }
    }

    public class ParsedBuildLogTextLine
    {
        public IList<ParsedBuildLogTextLineItem> Items { get; set; }

        public ParsedBuildLogTextLine()
        {
            this.Items = new List<ParsedBuildLogTextLineItem>();
        }
    }

    public class ParsedBuildLogText 
    {
        public IList<ParsedBuildLogTextLine> Items { get; set; }
        public string Type { get; set; }
        public ParsedBuildLogText(string type = "")
        {
            this.Type = type;
            this.Items = new List<ParsedBuildLogTextLine>();
        }

    }

    public static class BuildLogTextParser
    {
        public static ParsedBuildLogText Parse(string markup)
        {
            Match contentLookup = new Regex(@"<x-logParse\s*(?:type='(.*?)')?>\s*(.*?)\s*<\/x-logParse>", RegexOptions.Multiline).Match(markup);
            if (!contentLookup.Success)
                return null;


            string content = string.Empty;
            string type = string.Empty;

            if (contentLookup.Groups.Count == 2)
                content = contentLookup.Groups[1].Value;

            if (contentLookup.Groups.Count == 3) 
            {
                type = contentLookup.Groups[1].Value;
                content = contentLookup.Groups[2].Value;
            }
            ParsedBuildLogText result = new ParsedBuildLogText(type);

            MatchCollection linesLookup = new Regex("<x-logParseLine>\\s*(.+?)\\s*<\\/x-logParseLine>", RegexOptions.Multiline).Matches(content);

            foreach (Match line in linesLookup)
            {
                MatchCollection itemsLookup = new Regex("<x-logParseItem\\s*(?:type='(.*?)')?>(.+?)<\\/x-logParseItem>", RegexOptions.Multiline).Matches(line.Value);
                ParsedBuildLogTextLine outLine = new ParsedBuildLogTextLine();
                result.Items.Add(outLine);
                foreach (Match itemLookup in itemsLookup)
                {
                    string itemContent =string.Empty;
                    string itemType = string.Empty;
                    if (itemLookup.Groups.Count == 2)
                        itemContent = itemLookup.Groups[1].Value;

                    if (itemLookup.Groups.Count == 3)
                    {
                        itemType = itemLookup.Groups[1].Value;
                        itemContent = itemLookup.Groups[2].Value;
                    }
                    ParsedBuildLogTextLineItem outItem = new ParsedBuildLogTextLineItem {
                        Content = itemContent,
                        Type = itemType,
                    };

                    outLine.Items.Add(outItem);
                }
            }

            return result;
        }
    }
}
