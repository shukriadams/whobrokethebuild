using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Madscience.Perforce;
using System.Linq;

namespace Wbtb.Extensions.SourceServer.Perforce
{
    public class Perforce : Plugin, ISourceServerPlugin
    {
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
        }

        public ReachAttemptResult AttemptReach(Core.Common.SourceServer contextServer)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string shell = ContextPluginConfig.Config.Any(c => c.Key == "shell")? (string)ContextPluginConfig.Config.First(c => c.Key == "shell").Value : "cmd";
            PerforceUtils.ShellType shellType = shell == "cmd" ? PerforceUtils.ShellType.Cmd : PerforceUtils.ShellType.Sh;
            
            bool trust = false;

            // ensure that p4 is available
            if (!PerforceUtils.IsInstalled(shellType))
                return new ReachAttemptResult { Error = "p4 not installed" };

            if (contextServer.Config.Any(c => c.Key == "Trust")) 
                trust= contextServer.Config.First(c => c.Key == "Trust").Value.ToString().ToLower() == "true";
            
            try
            {
                if (trust)
                    PerforceUtils.Trust(host, shellType);

                PerforceUtils.VerifyCredentials(user, password, host, shellType);
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
            string shell = ContextPluginConfig.Config.Any(c => c.Key == "shell") ? (string)ContextPluginConfig.Config.First(c => c.Key == "shell").Value : "cmd";
            PerforceUtils.ShellType shellType = shell == "cmd" ? PerforceUtils.ShellType.Cmd : PerforceUtils.ShellType.Sh;

            IEnumerable<Change> changes = PerforceUtils.ChangesBetween(user, password, host, int.Parse(revisionStart), int.Parse(revisionEnd), shellType);
            return changes.Select(change => FromChange(change));
        }

        public Revision GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string host = contextServer.Config.First(c => c.Key == "Host").Value.ToString();
            string user = contextServer.Config.First(c => c.Key == "User").Value.ToString();
            string password = contextServer.Config.First(c => c.Key == "Password").Value.ToString();
            string shell = ContextPluginConfig.Config.Any(c => c.Key == "shell") ? (string)ContextPluginConfig.Config.First(c => c.Key == "shell").Value : "cmd";
            PerforceUtils.ShellType shellType = shell == "cmd" ? PerforceUtils.ShellType.Cmd : PerforceUtils.ShellType.Sh;

            // todo : add caching here, no need to hit p4 each time, history never changes
            return FromChange(PerforceUtils.Describe(user, password, host, int.Parse(revisionCode), shellType));
        }

        private static Revision FromChange(Change change)
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
