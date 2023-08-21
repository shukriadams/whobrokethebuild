using System;
using System.Net;
using System.Text;

namespace Wbtb.Core.Common
{
    public class MessageQueueHtppClient
    {
        private readonly ConfigurationBasic _configBasic;

        public MessageQueueHtppClient(ConfigurationBasic configBasic) 
        {
            _configBasic = configBasic;
        }

        public string Add(object data)
        {
            #pragma warning disable SYSLIB0014
            WebClient client = new WebClient();
            #pragma warning restore SYSLIB0014

            string json = JsonConvert.SerializeObject(data);
            byte[] postData = Encoding.ASCII.GetBytes(json);
            byte[] reply = client.UploadData($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeue", postData);
            string id = Encoding.ASCII.GetString(reply);

            return id;
        }

        public void EnsureAvailable()
        {
            try
            {
                #pragma warning disable SYSLIB0014
                WebClient client = new WebClient();
                #pragma warning restore SYSLIB0014

                string reply = client.DownloadString($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeue");
                if (!reply.Contains("online"))
                    throw new ConfigurationException($"Unexpected response from messagequeue server");
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("no connection could be made"))
                    throw new ConfigurationException("Messagequeue server not available");

                throw new ConfigurationException($"Unexpected response from messagequeue server", ex);
            }
        }

        public void AddConfig(Configuration config )
        {
            #pragma warning disable SYSLIB0014
            WebClient client = new WebClient();
            #pragma warning restore SYSLIB0014

            string json = JsonConvert.SerializeObject(config);
            byte[] postData = Encoding.ASCII.GetBytes(json);
            client.UploadData($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeueconfig", postData);
        }


        public string Retrieve(string id)
        {
            #pragma warning disable SYSLIB0014
            WebClient client = new WebClient();
            #pragma warning restore SYSLIB0014

            string reply = client.DownloadString($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeue/{id}");
            return reply;
        }

        public Configuration GetConfig()
        {
            #pragma warning disable SYSLIB0014
            WebClient client = new WebClient();
            #pragma warning restore SYSLIB0014

            string reply = client.DownloadString($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeueconfig");
            return JsonConvert.DeserializeObject<Configuration>(reply);
        }
    }
}
