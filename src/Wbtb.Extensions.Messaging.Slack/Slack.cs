using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.Slack
{
    public class SlackConfig
    { 
        public bool IsGroup { get;set; }
    }

    public class Slack : Plugin, IMessaging
    {
        #region FIELDS

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Config _config;

        private readonly PluginProvider _pluginProvider;

        #endregion

        #region CTORS

        public Slack(Config config, PluginProvider pluginProvider) 
        {
            _config = config;
            _pluginProvider = pluginProvider;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Attempt to reach slack to ensure config works
        /// </summary>
        /// <returns></returns>
        public ReachAttemptResult AttemptReach()
        {
            NameValueCollection data = new NameValueCollection();
            data["token"] = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            // list channels to ensure connection works
            
            try
            {
                dynamic response = ExecAPI("conversations.list", data);

                if (response.error != null && response.error.Value == "invalid_auth")
                    return new ReachAttemptResult { Error = "Slack credentials failed" };

                return new ReachAttemptResult { Reachable = true };
            }
            catch (Exception ex)
            {
                return new ReachAttemptResult { Exception = ex };
            }
        }

        public PluginInitResult InitializePlugin()
        {
            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Token"))
                throw new ConfigurationException("Missing item \"Token\"");

            return new PluginInitResult
            {
                Success = true,
                SessionId = Guid.NewGuid().ToString()
            };
        }


        public void ValidateAlertConfig(AlertConfig alertConfig)
        {
            if (string.IsNullOrEmpty(alertConfig.Key))
                throw new ConfigurationException("Slack detected alert with no \"Id\" value.");

            if (string.IsNullOrEmpty(alertConfig.Plugin))
                throw new ConfigurationException("Slack detected alert with no \"Plugin\" value.");
        }

        public string AlertBreaking(AlertHandler alertHandler, Build build)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();


            AlertConfig targetSlackConfig = null;

            if (!string.IsNullOrEmpty(alertHandler.User))
            { 
                User user = _config.Users.Single(u => u.Key == alertHandler.User);
                targetSlackConfig = user.Alert.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(alertHandler.Group))
            {
                Group group = _config.Groups.Single(u => u.Key == alertHandler.Group);
                targetSlackConfig = group.Alert.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = targetSlackConfig.Key;

            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
            {
                try
                {
                    slackId = this.GetUserChannelId(slackId);
                }
                catch (Exception ex)
                {
                    // log error
                    // mark message sent as failed, somwhere
                    Console.WriteLine(ex);
                    return null;
                }
            }

            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            string message = $"Build for {job.Name} broke at #{build.Identifier}.";
            dynamic attachment = new JObject();
            attachment.fallback = "dummy";
            attachment.text = message;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = message;
            data["attachments"] = JsonConvert.SerializeObject(attachment);

            dynamic response = ExecAPI("chat.postMessage", data);

            if (response.ok.Value)
            {
                // message sent
                return response.ts.Value;
            }
            else
            {
                // log error
                // mark message sent as failed, somwhere
                Console.WriteLine(response);
                return null;
            }
        }

        public string AlertPassing(AlertHandler alertHandler, Build build)
        {
            return string.Empty;
        }

        private dynamic ExecAPI(string apiFragment, NameValueCollection data, string method = "POST")
        {
            if (data == null)
                data = new NameValueCollection();

            WebClient client = new WebClient();
            string jsonResponse = Encoding.UTF8.GetString(client.UploadValues($"https://slack.com/api/{apiFragment}", method, data));
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);

        }

        private string GetUserChannelId(string slackUserId)
        {
            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();
            data["token"] = token;
            data["users"] = slackUserId;

            dynamic response = ExecAPI("conversations.open", data);
            if (response.ok.Value == true)
                return response.channel.id.Value;

            throw new Exception($"Failed to get user channel for slack userid {slackUserId}", response);
        }

        public string TestHandler(AlertHandler alertHandler)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();

            AlertConfig targetSlackConfig = null;
            if (!string.IsNullOrEmpty(alertHandler.User))
            {
                User user = _config.Users.Single(u => u.Key == alertHandler.User);
                targetSlackConfig = user.Alert.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(alertHandler.Group))
            {
                Group group = _config.Groups.Single(u => u.Key == alertHandler.Group);
                targetSlackConfig = group.Alert.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }
            
            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = targetSlackConfig.Key;


            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
            {
                try
                {
                    slackId = this.GetUserChannelId(slackId);
                }
                catch (Exception ex)
                {
                    // log error
                    // mark message sent as failed, somwhere
                    Console.WriteLine(ex);
                    return null;
                }
            }

            dynamic attachment = new JObject();
            string message = "This is a test message from WBTB to ensure connectivity works";
            attachment.fallback = message;
            attachment.text = message;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = "test message";
            data["attachments"] = Convert.ToString(attachment);

            dynamic response = ExecAPI("chat.postMessage", data);

            if (response.ok.Value)
            {
                // message sent
                return response.ts.Value;
            }
            else
            {
                // log error
                // mark message sent as failed, somwhere
                Console.WriteLine(response);
                return null;
            }
        }

        public void ListChannels()
        {
            NameValueCollection data = new NameValueCollection();
            data["token"] = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            // list channels to ensure connection works
            dynamic response = ExecAPI("conversations.list", data);
            IEnumerable<JToken> channels = Enumerable.ToList(response.channels);
            foreach (JToken channel in channels)
            { 
                Console.WriteLine(channel);
            }
        }

        public string DeleteAlert(object alertIdentifier)
        {
            return string.Empty;
        }

        #endregion
    }
}
