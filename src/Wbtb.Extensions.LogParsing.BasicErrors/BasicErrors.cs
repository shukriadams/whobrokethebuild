using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParserPlugin
    {
        #region PROPERTIES

        private readonly Logger _log;

        private readonly string Regex = @"^.*error.*$";

        private readonly Cache _cache;

        private readonly PluginProvider _pluginProvider;

        #endregion

        #region CTORS

        public BasicErrors(Logger log, Cache cache, PluginProvider pluginProvider) 
        {
            _log = log;
            _cache = cache;
            _pluginProvider = pluginProvider;
        }

        #endregion

        #region METHODS

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
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);

            // try for cache
            CachePayload lookup = _cache.Get(this, job, build, this.ContextPluginConfig.Key);
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
                    string errorStringKey = $"{Sha256.FromString(match.Value)}_error_instance";
                    string pluginKey = this.ContextPluginConfig.Manifest.Key;
                    errorStringCacheLookup = _cache.Get(pluginKey, errorStringKey);
                    PhraseOccurrence cachedOccurrence = null;

                    if (errorStringCacheLookup.Payload != null) 
                    {
                        try
                        {
                            cachedOccurrence = Newtonsoft.Json.JsonConvert.DeserializeObject<PhraseOccurrence>(errorStringCacheLookup.Payload);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(this, $"Failed to parse content of cached error occurrence at key {errorStringKey}, file will be overwritten.", ex);
                        }
                    }

                    if (cachedOccurrence == null)
                    {
                        cachedOccurrence = new PhraseOccurrence();
                        cachedOccurrence.Phrase = match.Value;
                    }

                    if (build.Status == BuildStatus.Passed)
                    {
                        // write error string to "safe error" cache
                        cachedOccurrence.Count ++;
                        _cache.Write(pluginKey, errorStringKey, Newtonsoft.Json.JsonConvert.SerializeObject(cachedOccurrence));
                        continue;
                    }

                    // ignore if error string is already in "safe error" cache
                    if (cachedOccurrence.Count > 2) // 5 is arbitrary score
                    {
                        ignoredErrors ++;
                        continue;
                    }

                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                if (ignoredErrors > 0) 
                    builder.AddItem($"Suppressed {ignoredErrors} additional occurrence(s).");

                result = builder.GetText();
            }

            _cache.Write(this, job, build, this.ContextPluginConfig.Key, result);
            return result;
        }

        #endregion
    }
}
