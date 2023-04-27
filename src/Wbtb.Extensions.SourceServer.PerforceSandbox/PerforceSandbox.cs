﻿using Madscience.Perforce;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;
using Wbtb.Core.Common.Utils;

namespace Wbtb.Extensions.SourceServer.PerforceSandbox
{
    public class PerforceSandbox : Plugin, ISourceServerPlugin
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
            //if (contextServer.Config == null)
            //    throw new ConfigurationException("Missing item \"Config\"");

            if (!contextServer.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing item \"Host\"");

            if (!contextServer.Config.Any(c => c.Key == "Password"))
                throw new ConfigurationException("Missing item \"Password\"");

            if (!contextServer.Config.Any(c => c.Key == "User"))
                throw new ConfigurationException("Missing item \"User\"");
        }

        public ReachAttemptResult AttemptReach(Core.Common.SourceServer contextServer)
        {
            // always succeed
            return new ReachAttemptResult { Reachable = true };
        }

        #endregion

        #region METHODS

        public IEnumerable<Revision> GetRevisionsBetween(Core.Common.SourceServer contextServer, string revisionStart, string revisionEnd)
        {
            return null;
        }

        public Revision GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string raw = ResourceHelper.LoadFromLocalJsonOrLocalResourceAsString(this.GetType(), $"./JSON/Revisions/{revisionCode}.json", $"JSON.Revisions.{revisionCode}.json");
            if (raw == null)
                return null;

            Change c = JsonConvert.DeserializeObject<Change>(raw);
            return FromChange(c);
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
