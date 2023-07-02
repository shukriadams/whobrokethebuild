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

            SimpleDI di = new SimpleDI();
            string regex = @"^.*error:.*$";
            // try for cache
            string hash = Sha256.FromString(regex + raw);
            Cache cache = di.Resolve<Cache>();
            string lookup = cache.Get(hash);
            if (lookup != null)
                return lookup;

            MatchCollection matches = new Regex(regex, RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);

            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach (Match match in matches)
                {
                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                lookup = builder.GetText();
            }
            else 
            {
                lookup = string.Empty;
            }

            cache.Write(hash, lookup);
            return lookup;
        }
    }
}
