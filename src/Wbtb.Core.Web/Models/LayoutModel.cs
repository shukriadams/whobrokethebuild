using Microsoft.AspNetCore.Mvc.RazorPages;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class LayoutModel : PageModel
    {
        public int ConsoleSize { get;set; }

        public bool IsAdmin { get; set; }

        public LayoutModel()
        { 
            this.ConsoleSize = ConfigKeeper.Instance.LiveConsoleSize;
            this.IsAdmin = true;
        }
    }
}
