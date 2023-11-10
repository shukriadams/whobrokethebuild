using System;
using System.Collections.Generic;
using System.Xml;

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
        public string Version { get; set; }

        public ParsedBuildLogText()
        {
            this.Type = string.Empty;
            this.Version = string.Empty;
            this.Items = new List<ParsedBuildLogTextLine>();
        }
    }

    public static class BuildLogTextParser
    {
        public static string ToInnerText(BuildLogParseResult result) 
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(result.ParsedContent);
                return xmlDoc.InnerText;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static ParsedBuildLogText Parse(string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return null;

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(markup);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

            string version = xmlDoc.DocumentElement.HasAttribute("version") ? xmlDoc.DocumentElement.GetAttribute("version") : string.Empty;
            string type = xmlDoc.DocumentElement.HasAttribute("type") ? xmlDoc.DocumentElement.GetAttribute("type") : string.Empty;

            ParsedBuildLogText result = new ParsedBuildLogText 
            {
                Version = version,
                Type = type
            };

            foreach (XmlElement row in xmlDoc.DocumentElement.ChildNodes)
            {
                ParsedBuildLogTextLine outLine = new ParsedBuildLogTextLine();
                result.Items.Add(outLine);

                foreach (XmlElement item in row.ChildNodes) 
                    outLine.Items.Add(new ParsedBuildLogTextLineItem
                    {
                        Content = item.InnerText,
                        Type = item.HasAttribute("type") ? item.GetAttribute("type") : string.Empty,
                    });
            }

            return result;
        }
    }
}
