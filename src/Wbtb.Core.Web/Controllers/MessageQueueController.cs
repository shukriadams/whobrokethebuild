using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MessageQueueController : Controller
    {
        private readonly IMessageQueue _messageQueue;

        private readonly ILogger<MessageQueueController> _log;

        public MessageQueueController(IMessageQueue messageQueue, ILogger<MessageQueueController> log)
        {
            _messageQueue = messageQueue;
            _log = log;
        }

        /// <summary>
        /// Gets and deletes a queued message
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [HttpGet("retrieve/{messageId}")]
        public ActionResult Retrieve(string messageId)
        {
            try
            {
                object message = _messageQueue.Retrieve(messageId);

                return new JsonResult(new
                {
                    success = new
                    {
                        message = message
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnknownError(ex.Message, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [HttpGet("config/{clientId}")]
        public ActionResult GetConfig(string clientId)
        {
            try
            {
                // validate clientId !
                return new JsonResult(new
                {
                    success = new
                    {
                        config = ConfigKeeper.Instance
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnknownError(ex.Message, 1);
            }
        }

        /// <summary>
        /// Puts a message in queue
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Add()
        {
            try
            {
                string json;
                using (StreamReader reader = new StreamReader(Request.Body))
                    json = await reader.ReadToEndAsync();

                object data = JsonConvert.DeserializeObject(json);

                string id = _messageQueue.Add(data);
                return new JsonResult(new
                {
                    success = new
                    {
                        id = id
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnknownError(ex.Message, 1);
            }
        }
    }
}
