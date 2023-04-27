using System.Net;
using Newtonsoft.Json;

namespace Wbtb.Extensions.BuildServer.Jenkins.Utils
{
    class JsonHelper
    {
        public static T Download<T>(string url)
        {
            WebClient webClient = new WebClient();
            string rawJson = webClient.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(rawJson);
        }
    }
}
