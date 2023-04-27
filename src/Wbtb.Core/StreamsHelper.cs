using System.IO;
using System.Text;

namespace Wbtb.Core
{
    public class StreamsHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (stream)
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Converts a stream content to string.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            return Encoding.Default.GetString(StreamToByteArray(stream));
        }
    }
}
