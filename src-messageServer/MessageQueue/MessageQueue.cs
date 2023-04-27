using Microsoft.Extensions.Caching.Memory;
using System;

namespace MessageQueue
{
    public class MessageQueue 
    {
        private readonly IMemoryCache _memoryCache;
        
        private static MessageQueue _messageQueue;

        public static MessageQueue Instance
        {
            get 
            {
                if (_messageQueue == null)
                    _messageQueue= new MessageQueue();

                return _messageQueue;
            }
        }

        public MessageQueue()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = new TimeSpan(0,0, 10) });
        }

        /// <summary>
        /// Adds object to queue, returns its unique id
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Add(object message)
        {
            string guid = Guid.NewGuid().ToString();
            
            if (Program.Persist)
                _memoryCache.Set(guid.ToString(), message);
            else
                _memoryCache.Set(guid.ToString(), message, new DateTimeOffset(DateTime.Now.AddSeconds(20)));

            return guid;
        }

        public string AddConfig(object message)
        {
            string guid = Guid.NewGuid().ToString();
            _memoryCache.Set("config", message);

            return guid;
        }

        /// <summary>
        /// Gets object from queue and deletes it.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public object Retrieve(string messageId)
        {
            object message;

            if (_memoryCache.TryGetValue(messageId, out message))
            {
                if (!Program.Persist)
                    _memoryCache.Remove(messageId);

                return message;
            }

            return null;
        }

        /// <summary>
        /// Gets object from queue and deletes it.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public object RetrieveConfig()
        {
            object message;

            if (_memoryCache.TryGetValue("config", out message))
            {
                return message;
            }

            return null;
        }

    }
}
