namespace Wbtb.Core.Common
{
    public class CachePayload
    {
        public string Payload { get; set; }
        
        /// <summary>
        /// If cache hit, the key at which the payload was found
        /// </summary>
        public string Key { get; set; }
    }
}
