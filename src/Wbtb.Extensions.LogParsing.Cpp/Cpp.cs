using System;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    public class Cpp : Plugin, ILogParser
    {
        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        public string Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // try to parse out C++ compile errors from log, these errors have the form like
            // D:\some\path\file.h(41,12): error C2143: syntax error: missing ';' before '*'
            // file path(line number): error code: description
            MatchCollection matches = new Regex(@"(.*?)(\(\d+,\d+\)): error\s?([A-Z]{1,2}[0-9]+):(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            // groups
            // 0 : all
            // 1 : file path
            // 2 : line nr in file
            // 3 : error code
            // 4 : description

            StringBuilder s = new StringBuilder();

            if (matches.Count > 0)
            {
                s.AppendLine("C++ compiler errors detected");
                foreach (Match match in matches)
                {
                    s.Append("<x-logParseLine>");
                    s.Append($"<x-logParseItem>{match.Groups[1]}</x-logParseItem>");
                    s.Append($"<x-logParseItem>{match.Groups[2]}</x-logParseItem>");
                    s.Append($"<x-logParseItem>{match.Groups[3]}</x-logParseItem>");
                    s.Append($"<x-logParseItem>{match.Groups[4]}</x-logParseItem>");
                    s.Append("</x-logParseLine>");
                }

                return s.ToString();
            }

            return null;
        }
    }
}
