using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewBuildInvolvement : BuildInvolvement
    {
        public Revision Revision { get; set;}

        public ViewBuild Build { get; set; }

        // user from MappedUserId. Null if not mapped.
        public ViewUser MappedUser { get; set; }

        public static ViewBuildInvolvement Copy(BuildInvolvement buildInvolvement)
        {
            if (buildInvolvement == null)
                return null;

            return new ViewBuildInvolvement
            {
                BuildId = buildInvolvement.BuildId,
                Comment = buildInvolvement.Comment,
                IsIgnoredFromBreakHistory = buildInvolvement.IsIgnoredFromBreakHistory,
                InferredRevisionLink = buildInvolvement.InferredRevisionLink,
                MappedUserId = buildInvolvement.MappedUserId,
                RevisionCode = buildInvolvement.RevisionCode,
                BlameScore = buildInvolvement.BlameScore,
                RevisionId = buildInvolvement.RevisionId,
                Id = buildInvolvement.Id
            };
        }

        public static IEnumerable<ViewBuildInvolvement> Copy(IEnumerable<BuildInvolvement> buildInvolvements)
        { 
            IList<ViewBuildInvolvement> list = new List<ViewBuildInvolvement>();
            
            foreach(BuildInvolvement buildInvolvement in buildInvolvements)
                list.Add(ViewBuildInvolvement.Copy(buildInvolvement));

            return list;
        }
    }
}
