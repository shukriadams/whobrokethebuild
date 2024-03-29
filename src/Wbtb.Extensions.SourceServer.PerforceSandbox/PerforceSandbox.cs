﻿using Madscience.Perforce;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.SourceServer.PerforceSandbox
{
    public class PerforceSandbox : Plugin, ISourceServerPlugin
    {
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
            // no config validation required
        }

        void ISourceServerPlugin.VerifyJobConfig(Job job, Core.Common.SourceServer contextServer)
        {
            // no config validation required
        }

        ReachAttemptResult ISourceServerPlugin.AttemptReach(Core.Common.SourceServer contextServer)
        {
            // always succeed
            return new ReachAttemptResult { Reachable = true };
        }

        #endregion

        #region METHODS

        IEnumerable<Revision> ISourceServerPlugin.GetRevisionsBetween(Job job, string revisionStart, string revisionEnd)
        {
            // cludge together an ugly way to do p4 range using saved json revision files. We assume revision nrs run in order etc etc
            int startRevision = int.Parse(revisionStart);
            int endRevision = int.Parse(revisionEnd);
            int currentRevision = startRevision + 1;
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Core.Common.SourceServer sourceServer = data.GetSourceServerById(job.SourceServerId);
            List<Revision> revisions = new List<Revision>();
            ISourceServerPlugin _this = this;

            while (currentRevision < endRevision) 
            {
                string path = $"JSON.Revisions.{currentRevision}.json";
                if (ResourceHelper.ResourceExists(this.GetType(), path)) 
                {
                    RevisionLookup revisionsLookup = _this.GetRevision(sourceServer, currentRevision.ToString());
                    if (revisionsLookup.Success)
                        revisions.Add(revisionsLookup.Revision);
                    else
                        throw new Exception(revisionsLookup.Error);
                }

                currentRevision++;
            }

            return revisions;
        }

        RevisionLookup ISourceServerPlugin.GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string rawChange = ResourceHelper.LoadFromLocalJsonOrLocalResourceAsString(this.GetType(), $"JSON.Revisions.{revisionCode}.json");
            if (rawChange == null)
                return new RevisionLookup { Error = "Revision not found"};

            Change change = JsonConvert.DeserializeObject<Change>(rawChange);
            return new RevisionLookup { Revision = FromChange(change), Success = true };
        }

        private static Revision FromChange(Change change)
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
