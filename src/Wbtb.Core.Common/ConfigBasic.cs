namespace Wbtb.Core.Common
{
    /// <summary>
    /// Simple configuration required for baseline functionality, including for plugins. Config is not checked as thoroughly as main config,
    /// config errors will not cause application start to halt, but will rather revert to reasonable defaults.
    /// </summary>
    public class ConfigBasic
    {
        private static ConfigBasic _basicConfig;

        public int MessageQueuePort { get;set; }

        public bool PersistCalls { get; set; }

        public string ProxyMode { get;set;}

        public ConfigBasic()
        {
         
            this.MessageQueuePort = 5001;
            this.PersistCalls = true;

            this.PersistCalls = EnvironmentVariableHelper.GetBool("WBTB_PERSISTCALLS", false);
            this.ProxyMode = EnvironmentVariableHelper.GetString("WBTB_PROXYMODE", "default");
        }

        public static ConfigBasic Instance
        {
            get 
            { 
                if (_basicConfig == null)
                    _basicConfig= new ConfigBasic();

                return _basicConfig;
            }
        }
    }
}
