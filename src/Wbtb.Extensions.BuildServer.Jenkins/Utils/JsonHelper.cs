using System.Net;
using Newtonsoft.Json;

namespace Wbtb.Extensions.BuildServer.Jenkins.Utils
{
    class JsonHelper
    {
        public static T Download<T>(string url)
        {
            #pragma warning disable SYSLIB0014
            WebClient client = new WebClient();
            #pragma warning restore SYSLIB0014

            string rawJson = client.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(rawJson);
        }
    }
}
