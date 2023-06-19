﻿using Madscience.Perforce;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

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
            // no config validation required
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
            // cludge together an ugly way to do p4 range using saved json revision files. We assume revision nrs run in order etc etc
            int startRevision = int.Parse(revisionStart);
            int endRevision = int.Parse(revisionEnd);
            int currentRevision = startRevision + 1;
            List<Revision> revisions = new List<Revision>();
            while (currentRevision < endRevision) 
            {
                string path = $"JSON.Revisions.{currentRevision}.json";
                if (ResourceHelper.ResourceExists(this.GetType(), path))
                    revisions.Add(this.GetRevision(contextServer, currentRevision.ToString()));

                currentRevision++;
            }
            return revisions;
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
