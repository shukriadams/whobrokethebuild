using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.AcmeGamesBlamer
{
    internal class AcmeGamesBlamer : Plugin, IPostProcessorPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        PostProcessResult IPostProcessorPlugin.Process(Build build)
        {
            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<BuildLogParseResult> logParseResults = data.GetBuildLogParseResultsByBuildId(build.Id);
            IEnumerable<BuildInvolvement> buildInvolvements = data.GetBuildInvolvementsByBuild(build.Id);

            // blame works at buildinvolvement level.

            // if log parse results are c++
            // ensure current source control is perforce
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to c++ file
            foreach (BuildLogParseResult result in logParseResults) 
            { 
                // PluginConfig pluginConfig = config.Plugins. result.LogParserPlugin
            }

            // if log parse results are blueprint
            // can we parse blueprint file path out of error message
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to blueprint file path
            return new PostProcessResult { };

            // if log parse results are jenkins
            // mark all users as not involved
            // add "jenkins" as build involvement, mark is cauase


            // if reach here, need to start guessing
            // if build involvement files are all gfx assets, mark user as unlikely to have caused issue
            
            // for build involvements
            // if revision contains cpp changes and error log is cpp, mark user as _possibly_ involved
            
            // note that alert will construct alerts based on blame level
            // if user definitely or possibly blamed, will alert user with logparse results related to that blame
            // will post public alert messages based on log parse results + blamed user

            // if no blames, will construct error message from all log parse results and send a public alert, but nothing to individual users
        }
    }
}
