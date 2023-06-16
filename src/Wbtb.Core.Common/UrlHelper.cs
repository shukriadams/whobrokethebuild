using System;

namespace Wbtb.Core.Common
{
    public class UrlHelper
    {
        private readonly Configuration _config;

        public UrlHelper(Configuration config) 
        {
            _config = config;
        }

        public string Build(Build build) 
        {
            string address = string.IsNullOrEmpty(_config.Address) ? $"http://localhost:{_config.Port}" : _config.Address;
            return new Uri(new Uri(address), $"/build/{build.Id}").ToString();
        }
    }
}
