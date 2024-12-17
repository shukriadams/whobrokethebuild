using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Madscience.Perforce;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Wbtb.Extensions.SourceServer.Perforce
{
    public class Perforce : Plugin, ISourceServerPlugin
    {
        #region FIELDS

        private readonly PersistPathHelper _persistPathHelper;
        
        private readonly ILogger _logger;

        #endregion

        #region CTORS

        public Perforce(PersistPathHelper persistPathHelper, ILogger log) 
        {
            _persistPathHelper = persistPathHelper;
            _logger = log; 
        }

        #endregion

        #region UTIL

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        void ISourceServerPlugin.VerifySourceServerConfig(Core.Common.SourceServer contextServer)
        {
            if (contextServer.Config == null)
                throw new ConfigurationException("Missing item \"Config\"");

            if (!contextServer.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing item \"Host\"");

            if (!contextServer.Config.Any(c => c.Key == "Password"))
                throw new ConfigurationException("Missing item \"Password\"");

            if (!contextServer.Config.Any(c => c.Key == "User"))
                throw new ConfigurationException("Missing item \"User\"");

            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            if (host.ToLower().StartsWith("ssl:") && !contextServer.Config.Any(c => c.Key == "TrustFingerprint")) 
                throw new ConfigurationException("P4 servers with SSL enabled required a \"TrustFingerprint\".");
        }

        void ISourceServerPlugin.VerifyJobConfig(Job job, Core.Common.SourceServer contextServer) 
        {
            if (job.Config == null)
                throw new ConfigurationException("Job Missing item \"Config\"");

            if (!job.Config.Any(c => c.Key == "p4depotRoot") && job.SourceServer == contextServer.Key)
                throw new ConfigurationException($"Plugin {this.ContextPluginConfig.Manifest.Key} requires each job it is linked to have a config item \"p4depotRoot\". Job \"{job.Name}\" does not. The value should be the depot + stream root for this job, followed with /..., example \"//mydepot/mystream/...\".  ");
        }

        ReachAttemptResult ISourceServerPlugin.AttemptReach(Core.Common.SourceServer contextServer)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string trust = string.Empty;
            if (contextServer.Config.Any(c => c.Key == "TrustFingerprint"))
                trust = contextServer.Config.First(c => c.Key == "TrustFingerprint").Value.ToString().ToLower();

            // ensure that p4 is available
            if (!PerforceUtils.IsInstalled())
                return new ReachAttemptResult { Error = "p4 not installed" };

            try
            {
                PerforceUtils.VerifyCredentials(user, password, host, trust);
                return new ReachAttemptResult { Reachable = true };
            }
            catch (Exception ex)
            {
                return new ReachAttemptResult { Exception = ex };
            }
        }

        #endregion

        #region METHODS

        IEnumerable<Revision> ISourceServerPlugin.GetRevisionsBetween(Job job, string revisionStart, string revisionEnd)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Core.Common.SourceServer contextServer = data.GetSourceServerById(job.SourceServerId);


            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string trust = string.Empty;
            if (contextServer.Config.Any(c => c.Key == "TrustFingerprint"))
                trust = contextServer.Config.First(c => c.Key == "TrustFingerprint").Value.ToString().ToLower();

            string depotRoot = job.Config.First(c => c.Key == "p4depotRoot").Value.ToString();

            IEnumerable<string> revisionNumbers = PerforceUtils.GetRawChangesBetween(user, password, host, trust, int.Parse(revisionStart), int.Parse(revisionEnd), depotRoot);
            IList<Revision> changes = new List<Revision>();
            ISourceServerPlugin _this = this;
            foreach (string revisionNumber in revisionNumbers) 
            {
                RevisionLookup revisionsLookup = _this.GetRevision(contextServer, revisionNumber);
                if (revisionsLookup.Success)
                    changes.Add(revisionsLookup.Revision);
                else
                    _logger.LogError($"Revison lookup for rev {revisionNumber} failed while doing range lookup between {revisionStart} and {revisionEnd} - {revisionsLookup.Error}.");
            }

            return changes;
        }

        RevisionLookup ISourceServerPlugin.GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string trust = string.Empty;
            if (contextServer.Config.Any(c => c.Key == "TrustFingerprint"))
                trust = contextServer.Config.First(c => c.Key == "TrustFingerprint").Value.ToString().ToLower();

            string persistPath = _persistPathHelper.GetPath(this.ContextPluginConfig, "revisions", $"{revisionCode}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            string describe = string.Empty;
            string readFromCacheWarning = string.Empty;
            if (File.Exists(persistPath))
            {
                describe = File.ReadAllText(persistPath);
                readFromCacheWarning = " Note, revision read from cached. If you had Perforce connection errors, you may need to purge plugin cache.";
            }
            else
            {
                try
                {
                    describe = PerforceUtils.GetRawDescribe(user, password, host, trust, int.Parse(revisionCode));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Invalid revision encoding"))
                        describe = "INVALID REVISION ENCODING";
                }
                File.WriteAllText(persistPath, describe);
            }

            if (describe == "INVALID REVISION ENCODING")
                return new RevisionLookup { Error = $"Revision encoding could not be read. Treating \"{revisionCode}\" as an invalid revision number.{readFromCacheWarning}" };

            if (string.IsNullOrEmpty(describe))
                return new RevisionLookup { Error = $"Revision was empty, assumed \"{revisionCode}\" is an invalid revision number.{readFromCacheWarning}" };

            Change change = PerforceUtils.ParseDescribe(describe);

            return new RevisionLookup { Revision = ChangeToRevision(change), Success = true } ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        private static Revision ChangeToRevision(Change change)
        {
            if (change == null)
                return null;

            IList<string> files = new List<string>();
            foreach (ChangeFile file in change.Files)
                files.Add(file.File);

            return new Revision
            {
                Code = change.Revision.ToString(),
                Created = change.Date,
                Description = change.Description,
                Files = files,
                User = change.User
            };
        }

        #endregion
    }
}
