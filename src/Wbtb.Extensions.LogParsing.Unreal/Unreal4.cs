using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Unreal
{
    public class Unreal4 : Plugin, ILogParserPlugin
    {
        private static string Find(string text, string regexPattern, RegexOptions options = RegexOptions.None, string defaultValue = "")
        {
            Match match = new Regex(regexPattern, options).Match(text);
            if (!match.Success || match.Groups.Count < 2)
                return defaultValue;

            return match.Groups[1].Value;
        }

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        string ILogParserPlugin.Parse(string raw)
        {
            // force unix things to standardize our processing
            IEnumerable<string> rawLines = raw
                .Replace("/\\/ g", "/")
                .Replace("/\r\n / g", "\n")
                .Split("\n");



            return null;
        }
    }
}
