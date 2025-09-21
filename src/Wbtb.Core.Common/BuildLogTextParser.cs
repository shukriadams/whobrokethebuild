using System;
using System.Xml;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Todo : refactor this class, merge into BuildLogParseResult etc, it is always used in the context of 
    /// other classes.
    /// </summary>
    public class BuildLogTextParser
    {
        public string ToInnerText(BuildLogParseResult result) 
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

        public Response<ParsedBuildLogText> Parse(string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return new Response<ParsedBuildLogText>();

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(markup);
            }
            catch(Exception ex)
            {
                return new Response<ParsedBuildLogText> { Error = $"Could not parse markup : {markup}, err {ex}" };
            }

            string version = xmlDoc.DocumentElement.HasAttribute("version") ? xmlDoc.DocumentElement.GetAttribute("version") : string.Empty;
            string key = xmlDoc.DocumentElement.HasAttribute("key") ? xmlDoc.DocumentElement.GetAttribute("key") : string.Empty;
            string type = xmlDoc.DocumentElement.HasAttribute("type") ? xmlDoc.DocumentElement.GetAttribute("type") : string.Empty;

            ParsedBuildLogText result = new ParsedBuildLogText 
            {
                Version = version,
                Key = key,
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

            return new Response<ParsedBuildLogText> { Value = result };
        }
    }
}
