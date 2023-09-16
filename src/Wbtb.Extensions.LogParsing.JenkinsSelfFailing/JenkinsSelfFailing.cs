using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.JenkinsSelfFailing
{
    public class JenkinsSelfFailing : Plugin, ILogParserPlugin
    {
        private readonly string RegexPattern = @"\b(at org.jenkinsci.|at jenkins.|at hudson.)\b.*";

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult{ 
                Success = true, 
                SessionId = Guid.NewGuid().ToString()
            };
        }

        string ILogParserPlugin.Parse(Build build, string raw)
        {
            SimpleDI di = new SimpleDI();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // try for cache
            string hash = Sha256.FromString(RegexPattern + fullErrorLog);
            Cache cache = di.Resolve<Cache>();
            CachePayload cacheLookup = cache.Get(this, hash);
            if (cacheLookup.Payload != null)
                return cacheLookup.Payload;

            // look for following matches
            // at org.jenkinsci
            // at jenkins.
            // at hudson.
            // at java

            MatchCollection matches = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            string result = string.Empty;
            if (matches.Any()) 
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach(Match match in matches) 
                {
                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                result =  builder.GetText();
            }

            cache.Write(this, hash, result);
            return result;
        }
    }
}
