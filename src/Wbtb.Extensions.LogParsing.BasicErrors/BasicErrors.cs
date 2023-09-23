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

        string ILogParserPlugin.Parse(Build build,string raw)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);

            // try for cache
            string hash = Sha256.FromString(Regex + raw);
            Cache cache = di.Resolve<Cache>();
            CachePayload lookup = cache.Get(this, job, build, hash);
            if (lookup.Payload != null)
                return lookup.Payload;

            MatchCollection matches = new Regex(Regex, RegexOptions.IgnoreCase|RegexOptions.Multiline).Matches(raw);

            string result = string.Empty;
            if (matches.Any()) 
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                int ignoredErrors = 0;

                foreach (Match match in matches)
                {
                    CachePayload errorStringCacheLookup = null;
                    string errorStringKey = Sha256.FromString(match.Value);
                    errorStringCacheLookup = cache.Get(this, job, build, errorStringKey);
                    int cacheCount = 0;

                    if (errorStringCacheLookup.Payload != null) 
                    { 
                        int.TryParse(errorStringCacheLookup.Payload, out cacheCount);
                    }

                    if (build.Status == BuildStatus.Passed)
                    {
                        // write error string to "safe error" cache
                        cacheCount++;
                        cache.Write(this, job, build, errorStringKey, cacheCount.ToString());
                        continue;
                    }

                    // ignore if error string is already in "safe error" cache
                    if (cacheCount > 2) // 5 is arbitrary score
                    {
                        ignoredErrors++;
                        continue;
                    }

                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                if (ignoredErrors > 0) 
                    builder.AddItem($"Suppressed {ignoredErrors} additional occurrence(s).");

                result = builder.GetText();
            }

            cache.Write(this, job, build, hash, result);
            return result;
        }
    }
}
