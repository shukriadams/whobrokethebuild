using System.Collections.Generic;
using System.Linq;

namespace Wbtb.Core.Common
{
    public class MutationHelper
    {
        private PluginProvider _pluginProvider;

        public MutationHelper(PluginProvider pluginProvider) 
        {
            _pluginProvider = pluginProvider;
        }

        public string GetBuildMutation(Build build)
        {
            IDataPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<BuildLogParseResult> previousBuildLogParseResults = datalayer.GetBuildLogParseResultsByBuildId(build.Id);
            string mutation = Sha256.FromString(string.Join(string.Empty, previousBuildLogParseResults.Select(r => r.ParsedContent)));
            return mutation;
        }
    }
}
