using System.Text.RegularExpressions;
using System.Text;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParser
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

            MatchCollection matches = new Regex(@"^.*error:.*$", RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);
            StringBuilder s = new StringBuilder();

            if (!matches.Any())
                return string.Empty;

            foreach (Match match in matches)
            {
                s.Append("<x-logParseLine>");
                s.Append($"<x-logParseItem>{match.Value}</x-logParseItem>");
                s.Append("</x-logParseLine>");
            }

            return s.ToString();
        }
    }
}
