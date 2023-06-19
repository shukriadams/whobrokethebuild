using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Madscience.Perforce;
using System.Linq;
using System.IO;

namespace Wbtb.Extensions.SourceServer.Perforce
{
    public class Perforce : Plugin, ISourceServerPlugin
    {
        #region FIELDS

        private readonly PersistPathHelper _persistPathHelper;

        #endregion

        #region CTORS

        public Perforce(PersistPathHelper persistPathHelper) 
        {
            _persistPathHelper = persistPathHelper;
        }

        #endregion

        #region UTIL

        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        public void VerifySourceServerConfig(Core.Common.SourceServer contextServer)
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

        public ReachAttemptResult AttemptReach(Core.Common.SourceServer contextServer)
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

        public IEnumerable<Revision> GetRevisionsBetween(Core.Common.SourceServer contextServer, string revisionStart, string revisionEnd)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string trust = string.Empty;
            if (contextServer.Config.Any(c => c.Key == "TrustFingerprint"))
                trust = contextServer.Config.First(c => c.Key == "TrustFingerprint").Value.ToString().ToLower();

            IEnumerable<string> revisionNumbers = PerforceUtils.GetRawChangesBetween(user, password, host, trust, int.Parse(revisionStart), int.Parse(revisionEnd));
            IList<Revision> changes = new List<Revision>();
            foreach (string revisionNumber in revisionNumbers)
                changes.Add(GetRevision(contextServer, revisionNumber));

            return changes;
        }

        public Revision GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string trust = string.Empty;
            if (contextServer.Config.Any(c => c.Key == "TrustFingerprint"))
                trust = contextServer.Config.First(c => c.Key == "TrustFingerprint").Value.ToString().ToLower();

            string persistPath = _persistPathHelper.GetPath(this.ContextPluginConfig, "revisions", $"{revisionCode}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            string describe = string.Empty;

            if (File.Exists(persistPath))
            {
                describe = File.ReadAllText(persistPath);
            }
            else
            {
                describe = PerforceUtils.GetRawDescribe(user, password, host, trust, int.Parse(revisionCode));
                File.WriteAllText(persistPath, describe);
            }

            Change change = PerforceUtils.ParseDescribe(describe);

            return ChangeToRevision(change);
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

            return new Revision
            {
                Code = change.Revision.ToString(),
                Created = change.Date,
                Description = change.Description,
                Files = change.Files.Select(r => r.File),
                User = change.User
            };
        }

        #endregion
    }
}
