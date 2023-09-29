using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace Wbtb.Core.Web
{
    public class ViewJobBanner
    {
        public List<HtmlString> BreadCrumbs = new List<HtmlString>();

        public ViewJob Job { get; set; }

        public ViewJobBanner() 
        {
            this.BreadCrumbs = new List<HtmlString>();
        }
    }
}
