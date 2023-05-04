namespace Wbtb.Core.Common
{
    public class PluginInitResult
    {
        public string SessionId { get;set;}

        public bool Success { get; set; }

        public string Description { get; set; }

        public PluginInitResult()
        { 
            Description = string.Empty;
        }
    }
}
