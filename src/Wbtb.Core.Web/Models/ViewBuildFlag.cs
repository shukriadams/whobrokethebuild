using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewBuildFlag : BuildFlag
    {
        public static ViewBuildFlag Copy(BuildFlag buildflag)
        {
            if (buildflag == null)
                return null;

            return new ViewBuildFlag
            {
                BuildId = buildflag.BuildId,
                Flag = buildflag.Flag,
                CreatedUtc = buildflag.CreatedUtc,
                Description = buildflag.Description,
                Ignored = buildflag.Ignored,
                Id = buildflag.Id
            };
        }

        public static PageableData<ViewBuildFlag> Copy(PageableData<BuildFlag> builds)
        {
            return new PageableData<ViewBuildFlag>(
                builds.Items.Select(r => ViewBuildFlag.Copy(r)),
                builds.PageIndex,
                builds.PageSize,
                builds.TotalItemCount);
        }
    }
}
