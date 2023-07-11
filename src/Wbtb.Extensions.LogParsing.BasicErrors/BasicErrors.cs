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

        LogParsePickupResult ILogParserPlugin.Pickup(string raw)
        {
            SimpleDI di = new SimpleDI();
            
            // try for cache
            string hash = Sha256.FromString(Regex + raw);
            Cache cache = di.Resolve<Cache>();
            string lookup = cache.Get(this, hash);
            if (lookup != null)
                return new LogParsePickupResult
                {
                    Result = lookup,
                    Found = true
                };

            return new LogParsePickupResult();
        }

        void ILogParserPlugin.Parse(string raw)
        {
            MatchCollection matches = new Regex(Regex, RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);

            string result = string.Empty;
            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach (Match match in matches)
                {
                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                result = builder.GetText();
            }

            SimpleDI di = new SimpleDI();
            Cache cache = di.Resolve<Cache>();
            string hash = Sha256.FromString(Regex + raw);
            cache.Write(this, hash, result);
        }
    }
}
