using Microsoft.Extensions.Caching.Memory;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class MessageQueue : IMessageQueue
    {
        private readonly IMemoryCache _memoryCache;

        public MessageQueue(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Adds object to queue, returns its unique id
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Add(object message)
        { 
            string guid = Guid.NewGuid().ToString();
            _memoryCache.Set(guid.ToString(), message, new DateTimeOffset(DateTime.Now.AddSeconds(20)));
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

            if (!_memoryCache.TryGetValue(messageId, out message))
            {
                _memoryCache.Remove(messageId);
                return message;
            }

            return null;
        }
    }
}
