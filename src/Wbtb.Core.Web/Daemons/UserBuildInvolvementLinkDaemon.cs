using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Daemon for connecting a defined and mapped WBTB user to the string username associated with a source control revision
    /// </summary>
    public class UserBuildInvolvementLinkDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Config _config;

        #endregion

        #region CTORS

        public UserBuildInvolvementLinkDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Config>();
            _pluginProvider = di.Resolve<PluginProvider>(); 

        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        private void Work()
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                foreach (Job cfgJob in buildServer.Jobs.Where(j => !string.IsNullOrEmpty(j.SourceServerId)))
                {
                    try
                    {
                        Job job = dataLayer.GetJobByKey(cfgJob.Key);
                        SourceServer sourceServer = dataLayer.GetSourceServerById(job.SourceServerId);
                        ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                        IEnumerable<BuildInvolvement> buildInvolvementsWithoutUser = dataLayer.GetBuildInvolvementsWithoutMappedUser(job.Id);

                        foreach (BuildInvolvement buildInvolvementWithoutUser in buildInvolvementsWithoutUser)
                        {
                            try
                            {
                                // we assume revision is already set here
                                Revision revision = dataLayer.GetRevisionById(buildInvolvementWithoutUser.RevisionId);

                                // build involvement now contains the name of the user in the source control system from which the build
                                // revision originates - use this to try to find the user in the config
                                User matchingUser = _config.Users
                                    .FirstOrDefault(r => r.SourceServerIdentities
                                        .Any(r => r.Name == revision.User));

                                User user = null;
                                if (matchingUser != null)
                                    user = dataLayer.GetUserByKey(matchingUser.Key);

                                if (user == null)
                                {
                                    dataLayer.SaveBuildFlag(new BuildFlag { 
                                        BuildId = buildInvolvementWithoutUser.BuildId,
                                        Description = $"User {revision.User} for buildinvolvement does not exist. Add user and rerun import",
                                        Flag = BuildFlags.BuildUserNotDefinedLocally
                                    });

                                    Console.WriteLine($"User {revision.User} for buildinvolvement does not exist. Add user and rerun import");
                                }
                                else
                                {
                                    buildInvolvementWithoutUser.MappedUserId = user.Id;
                                    Console.WriteLine($"Linked user \"{user.Key}\" to build \"{buildInvolvementWithoutUser.BuildId}\".");
                                }

                                dataLayer.SaveBuildInvolement(buildInvolvementWithoutUser);
                            }
                            catch (Exception ex)
                            {
                                // todo : need a better way of handling failed actions on a multistage logic. Ideally, we'd mark something as failed only after
                                // it has failed several times, and then all records need to be able to clearly store info about the failure.
                                if (ex.Message.ToLower().Contains("no such changelist"))
                                {
                                    dataLayer.SaveBuildFlag(new BuildFlag
                                    {
                                        BuildId = buildInvolvementWithoutUser.BuildId,
                                        Description = $"Buildinvolvement {buildInvolvementWithoutUser.Id} defines a revsion that could not be retrieve from attached source control system. Abandoning processing",
                                        Flag = BuildFlags.RevisionNotFound
                                    });
                                    Console.WriteLine($"Revision {buildInvolvementWithoutUser.RevisionCode} does not exist, marking as invalid");
                                } 
                                else
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import jobs/logs for {cfgJob.Key} from buildserver {buildServer.Key}: {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
