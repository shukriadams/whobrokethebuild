using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Wbtb.Core.Web
{
    public class JobsPageModel : LayoutModel
    {
        public string Title { get; set; }

        public IEnumerable<ViewJob> Jobs { get; set; }

        public CommonModel Common { get; set; }

        public bool AnyJobUsingBanners { get; set; }
    }
}
