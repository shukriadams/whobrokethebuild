using System.Net;
using Newtonsoft.Json;

namespace Wbtb.Extensions.BuildServer.Jenkins.Utils
{
    class JsonHelper
    {
        public static T Download<T>(string url)
        {
            WebClient client = new WebClient();
            string rawJson = client.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(rawJson);
        }
    }
}
