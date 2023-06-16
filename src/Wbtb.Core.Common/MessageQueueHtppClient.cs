using System;
using System.Net;
using System.Text;

namespace Wbtb.Core.Common
{
    public class MessageQueueHtppClient
    {
        private readonly ConfigBasic _configBasic;

        public MessageQueueHtppClient(ConfigBasic configBasic) 
        {
            _configBasic = configBasic;
        }

        public string Add(object data)
        { 
            WebClient client = new WebClient();
            
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
                WebClient client = new WebClient();
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
            WebClient client = new WebClient();

            string json = JsonConvert.SerializeObject(config);
            byte[] postData = Encoding.ASCII.GetBytes(json);
            client.UploadData($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeueconfig", postData);
        }


        public string Retrieve(string id)
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeue/{id}");
            return reply;
        }

        public Configuration GetConfig()
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString($"http://localhost:{_configBasic.MessageQueuePort}/api/v1/messagequeueconfig");
            return JsonConvert.DeserializeObject<Configuration>(reply);
        }
    }
}
