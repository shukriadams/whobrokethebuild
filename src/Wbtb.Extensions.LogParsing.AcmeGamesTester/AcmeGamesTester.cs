using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.AcmeGamesTester
{
    public class AcmeGamesTester : Plugin, ILogParserPlugin
    {
        #region PROPERTIES

        private readonly Logger _log;

        #endregion

        #region CTORS

        public AcmeGamesTester(Logger log)
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

        string ILogParserPlugin.Parse(Build build, string raw)
        {
            string chunkDelimiter = string.Empty;
            if (ContextPluginConfig.Config.Any(r => r.Key == "SectionDelimiter"))
                chunkDelimiter = ContextPluginConfig.Config.First(r => r.Key == "SectionDelimiter").Value.ToString();

            // example of an error :
            // LogLinker: Error: [AssetLog] D:\workdir\IronBird\Content\Acme\Tests\ThingTester.umap: Failed import: class 'AngelGrinder' name '_Sprucker' outer 'BP_Luncher'. There is another object (of 'BP_Luncher' class) at the path."
            // group 1 : filepath
            // group 2 : error explanation
            string errorRegex = @"LogLinker: Error: \[.*?\] (.*)?: (.*?: .*)";

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Cache cache = di.Resolve<Cache>();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            CachePayload internaleErrorCacheLookup = cache.Get(this, job, build, this.ContextPluginConfig.Key);
            if (internaleErrorCacheLookup.Payload != null)
                return internaleErrorCacheLookup.Payload;

            IEnumerable<string> chunks = null;
            if (string.IsNullOrEmpty(chunkDelimiter))
                chunks = new List<string> { fullErrorLog };
            else
                chunks = fullErrorLog.Split(chunkDelimiter);

            StringBuilder result = new StringBuilder();
            foreach (string chunk in chunks)
            {
                MatchCollection matches = new Regex(errorRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Groups[1].Value, "path");
                        builder.AddItem(match.Groups[2].Value, "description");
                        builder.NewLine();
                    }

                    result.Append(builder.GetText());
                }
            }

            string flattened = result.ToString();
            cache.Write(this, job, build, this.ContextPluginConfig.Key, flattened);
            return flattened;
        }

        #endregion
    }
}
