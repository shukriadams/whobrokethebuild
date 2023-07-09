using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParserPlugin
    {
        private readonly string Regex = @"^.*error:.*$";

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        string ILogParserPlugin.ParseAndCache(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            SimpleDI di = new SimpleDI();
            
            // try for cache
            string hash = Sha256.FromString(Regex + raw);
            Cache cache = di.Resolve<Cache>();
            string lookup = cache.Get(this, hash);
            if (lookup != null)
                return lookup;

            lookup = ((ILogParserPlugin)this).Parse(raw);
            cache.Write(this, hash, lookup);
            return lookup;
        }

        string ILogParserPlugin.Parse(string raw)
        {
            MatchCollection matches = new Regex(Regex, RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);

            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach (Match match in matches)
                {
                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                return builder.GetText();
            }
            else 
            {
                return string.Empty;
            }
        }
    }
}
