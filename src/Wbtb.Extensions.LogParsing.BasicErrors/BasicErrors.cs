using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParserPlugin
    {
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
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            MatchCollection matches = new Regex(@"^.*error:.*$", RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);

            if (!matches.Any())
                return string.Empty;

            BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
            foreach (Match match in matches)
            {
                builder.AddItem(match.Value);
                builder.NewLine();
            }
    
            return builder.GetText();
        }
    }
}
