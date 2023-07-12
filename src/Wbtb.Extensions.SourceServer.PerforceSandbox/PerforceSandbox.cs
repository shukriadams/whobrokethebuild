using Madscience.Perforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        void ISourceServerPlugin.VerifyJobConfig(Job job)
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
                    revisions.Add(_this.GetRevision(sourceServer, currentRevision.ToString()));

                currentRevision++;
            }

            return revisions;
        }

        Revision ISourceServerPlugin.GetRevision(Core.Common.SourceServer contextServer, string revisionCode)
        {
            string rawChange = ResourceHelper.LoadFromLocalJsonOrLocalResourceAsString(this.GetType(), $"JSON.Revisions.{revisionCode}.json");
            if (rawChange == null)
                return null;

            Change change = JsonConvert.DeserializeObject<Change>(rawChange);
            Revision revision = FromChange(change);

            // try to get workspace
            string rawClient = ResourceHelper.LoadFromLocalJsonOrLocalResourceAsString(this.GetType(), $"JSON.Clients.{change.Workspace}.txt");
            if (rawClient != null) 
            {
                Client client = PerforceUtils.ParseClient(rawClient);
                if (client != null) 
                {
                    // try to calculate localpPath of file based on stream mapping. NOTE! This assumes that workspace setup in which the revision 
                    // was created is the same as the workspace setup in which the code was built.
                    foreach (ChangeFile changefile in change.Files) 
                    {
                        string localPath = string.Empty;
                        foreach (ClientView view in client.Views) 
                        {
                            string remoteFragment = view.Remote.Replace("...", "");
                            string localFragment  = view.Local.Replace("...", "");
                            localFragment = localFragment.Replace($"//{client.Name}", client.Root);
                            if (changefile.File.StartsWith(remoteFragment)) 
                            {
                                localPath = changefile.File.Replace(remoteFragment, string.Empty); // clip off everything above workspace root, we don't care about that.
                                localPath = Regex.Replace(localPath, @"\\", "/"); // force local path to unix format so we have to worry about one path format 

                                RevisionFile rfile = revision.Files.Single(f => f.Path == changefile.File);
                                rfile.LocalPath = localPath;
                            }
                        }
                    }
                }
            }

            return revision;
        }

        private static Revision FromChange(Change change)
        {
            if (change == null)
                return null;

            IList<RevisionFile> files = new List<RevisionFile>();
            foreach (ChangeFile file in change.Files)
                files.Add(new RevisionFile
                {
                    Path = file.File
                });

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
