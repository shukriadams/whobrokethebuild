namespace Wbtb.Core.Common
{
    public class ConfigKeeper
    {
        private static Config _instance;

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                    throw new ConfigurationException("Config not initialized. Call Core.Init on app start");

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }

}
