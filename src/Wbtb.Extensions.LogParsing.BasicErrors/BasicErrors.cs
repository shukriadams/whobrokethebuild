using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParserPlugin
    {
        private readonly string Regex = @"^.*error.*$";

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
            SimpleDI di = new SimpleDI();

            // try for cache
            string hash = Sha256.FromString(Regex + raw);
            Cache cache = di.Resolve<Cache>();
            CachePayload lookup = cache.Get(this, hash);
            if (lookup.Payload != null)
                return lookup.Payload;

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

            cache.Write(this, hash, result);
            return result;
        }
    }
}
