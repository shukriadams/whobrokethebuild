using Microsoft.AspNetCore.Mvc.RazorPages;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class LayoutModel : PageModel
    {
        public int ConsoleSize { get;set; }

        public bool IsAdmin { get; set; }

        public Configuration Config { get; set; }


        public LayoutModel()
        { 

            SimpleDI di = new SimpleDI();
            this.Config = di.Resolve<Configuration>();

            this.ConsoleSize = this.Config.LiveConsoleSize;
            this.IsAdmin = true;
        }
    }
}
