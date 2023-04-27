using System.Linq;

namespace Wbtb.Extensions.BuildServer.Jenkins
{
    public class UrlHelper
    {
        /// <summary>
        /// from https://stackoverflow.com/a/7993235/1216792
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static string Join(string baseUrl, params string[] segments)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            if (segments == null || segments.Length == 0)
                return baseUrl;

            segments = segments.Select(r => r).ToArray();

            string url = segments.Aggregate(baseUrl, (current, segment) => $"{current.TrimEnd('/')}/{segment.TrimStart('/')}");
            
            // url = HttpUtility.UrlEncode(url);

            return url;
        }
    }
}
