﻿using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    internal class BasicErrors : Plugin, ILogParserPlugin
    {
        #region PROPERTIES

        private readonly ILogger _log;

        private readonly string Regex = @"^.*error.*$";

        #endregion

        #region CTORS

        public BasicErrors(ILogger log) 
        {
            _log = log;
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
                    string errorStringKey = $"{Sha256.FromString(match.Value)}_error_instance" ;
                    string pluginKey = this.ContextPluginConfig.Manifest.Key;
                    errorStringCacheLookup = cache.Get(pluginKey, errorStringKey);
                    PhraseOccurrence cachedOccurence = null;

                    if (errorStringCacheLookup.Payload != null) 
                    {
                        try
                        {
                            cachedOccurence = Newtonsoft.Json.JsonConvert.DeserializeObject<PhraseOccurrence>(errorStringCacheLookup.Payload);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError($"Failed to parse concents of cached error occurrence at key {errorStringKey} : {ex.Message}. File will be overwritten.");
                        }
                    }

                    if (cachedOccurence == null)
                    {
                        cachedOccurence = new PhraseOccurrence();
                        cachedOccurence.Phrase = match.Value;
                    }

                    if (build.Status == BuildStatus.Passed)
                    {
                        // write error string to "safe error" cache
                        cachedOccurence.Count ++;
                        cache.Write(pluginKey, errorStringKey, Newtonsoft.Json.JsonConvert.SerializeObject(cachedOccurence));
                        continue;
                    }

                    // ignore if error string is already in "safe error" cache
                    if (cachedOccurence.Count > 2) // 5 is arbitrary score
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

            cache.Write(this, job, build, hash, result);
            return result;
        }

        #endregion
    }
}
