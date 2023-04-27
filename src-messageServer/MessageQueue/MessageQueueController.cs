using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Text;

namespace MessageQueue
{
    [RestResource]
    public class MessageQueueController
    {

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/v1/messagequeue")]
        public IHttpContext Poke(IHttpContext context)
        {
            try
            {
                context.Response.SendResponse("messagequeue online");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/v1/messagequeue/(.*)?")]
        public IHttpContext Retrieve(IHttpContext context)
        {
            try
            {
                string messageId = context.Request.PathParameters["p0"];
                object message = MessageQueue.Instance.Retrieve(messageId);
                string json = JsonConvert.SerializeObject(message);

                if (Program.Verbose)
                    Console.WriteLine($"Retrieving message {messageId}");

                context.Response.ContentType = ContentType.JSON;
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = json.Length;
                context.Response.SendResponse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/v1/messagequeueconfig")]
        public IHttpContext RetrieveConfig(IHttpContext context)
        {
            try
            {
                object message = MessageQueue.Instance.RetrieveConfig();
                string json = JsonConvert.SerializeObject(message);

                if (Program.Verbose)
                    Console.WriteLine($"Retrieving config");

                context.Response.ContentType = ContentType.JSON;
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = json.Length;
                context.Response.SendResponse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/api/v1/messagequeue")]
        public IHttpContext Add(IHttpContext context)
        {
            try
            {
                string body = context.Request.Payload;
                object data = JsonConvert.DeserializeObject(body);
                string messageId = MessageQueue.Instance.Add(data);

                if (Program.Verbose)
                    Console.WriteLine($"Adding message {messageId}");

                context.Response.ContentType = ContentType.TEXT;
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = messageId.Length;
                context.Response.SendResponse(messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/api/v1/messagequeueconfig")]
        public IHttpContext AddConfig(IHttpContext context)
        {
            try
            {
                string body = context.Request.Payload;
                object data = JsonConvert.DeserializeObject(body);
                MessageQueue.Instance.AddConfig(data);

                if (Program.Verbose)
                    Console.WriteLine($"Adding config");

                context.Response.ContentType = ContentType.TEXT;
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = "added".Length;
                context.Response.SendResponse("added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return context;
        }

    }
}