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
        public string SlackId { get; set; }
        public bool IsGroup { get;set; }
    }

    public class Slack : Plugin, IMessaging
    {
        #region FIELDS

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Config _config;

        private readonly PluginProvider _pluginProvider;

        private readonly UrlHelper _urlHelper;

        #endregion

        #region CTORS

        public Slack(Config config, UrlHelper urlHelper, PluginProvider pluginProvider) 
        {
            _config = config;
            _urlHelper = urlHelper;
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

            try
            {
                // list channels to ensure connection works
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
                throw new ConfigurationException("Missing Config item \"Token\"");

           // if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Secret"))
           //     throw new ConfigurationException("Missing Config item \"Secret\"");

            if (string.IsNullOrEmpty(_config.Address))
                throw new ConfigurationException("Slack alerts require WBTB Core \"Address\" to be set, these are used to generate links.");

            return new PluginInitResult
            {
                Success = true,
                SessionId = Guid.NewGuid().ToString()
            };
        }

        public void ValidateAlertConfig(MessageConfiguration alertConfig)
        {
            if (string.IsNullOrEmpty(alertConfig.Plugin))
                throw new ConfigurationException("Slack detected alert with no \"Plugin\" value.");
        }

        private string AlertKey(string slackChannelId, string jobId, string incidentBuildId)
        {
            return $"buildStatusAlert_slack_{slackChannelId}_job{jobId}_incident{incidentBuildId}";
        }


        public string AlertBreaking(MessageHandler alertHandler, Build incidentBuild)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();

            MessageConfiguration targetSlackConfig = null;

            if (!string.IsNullOrEmpty(alertHandler.User))
            {
                User user = _config.Users.Single(u => u.Key == alertHandler.User);
                targetSlackConfig = user.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(alertHandler.Group))
            {
                Group group = _config.Groups.Single(u => u.Key == alertHandler.Group);
                targetSlackConfig = group.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (targetSlackConfig == null)
                throw new Exception("alerthandler has neither user nor group");

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = config.SlackId;

            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
            {
                try
                {
                    slackId = this.GetUserChannelId(slackId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }

            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = dataLayer.GetJobById(incidentBuild.JobId);
            IEnumerable<BuildLogParseResult> parseResults = dataLayer.GetBuildLogParseResultsByBuildId(incidentBuild.Id);

            // check if alert has already been sent
            
            
            string key = AlertKey(slackId, job.Id, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);
            if (storeItem != null)
                return null;

            string message = $"Build broke at #{incidentBuild.Identifier}.";
            string errors = string.Empty;
            if (parseResults.Any())
                foreach (BuildLogParseResult parseResult in parseResults.Where(p => !string.IsNullOrEmpty(p.ParsedContent))) 
                {
                    if (parseResult.ParsedContent.Contains("<x-logParseLine>"))
                    {
                        errors += "```";
                        System.Text.RegularExpressions.MatchCollection linesLookup = new System.Text.RegularExpressions.Regex("<x-logParseLine>(.+?)<\\/x-logParseLine>", System.Text.RegularExpressions.RegexOptions.Multiline).Matches(parseResult.ParsedContent);
                        foreach (System.Text.RegularExpressions.Match line in linesLookup) 
                        {
                            System.Text.RegularExpressions.MatchCollection itemsLookup = new System.Text.RegularExpressions.Regex("<x-logParseItem>(.+?)<\\/x-logParseItem>", System.Text.RegularExpressions.RegexOptions.Multiline).Matches(line.Value);
                            foreach(System.Text.RegularExpressions.Match item in itemsLookup)
                                if (item.Groups.Count > 0)
                                    errors += $"{item.Groups[1].Value}";
                            
                            errors += "\n";
                        }
                        errors += "```";
                    }
                    else 
                    {
                        errors += $"```{parseResult.ParsedContent}```";
                    }
                }

            dynamic attachment = new JObject();
            attachment.title = $"{job.Name} is DOWN";
            attachment.fallback = " ";
            attachment.color = "#D92424";
            attachment.text = message+errors;
            attachment.title_link = _urlHelper.Build(incidentBuild);

            var attachments = new JArray(1);
            attachments[0] = attachment;


            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);

            dynamic response = ExecAPI("chat.postMessage", data);

            if (response.ok.Value)
            {
                // store message info and proof of sending
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = $"buildStatusAlert_slack_{slackId}_job{job.Id}_incident{incidentBuild.IncidentBuildId}",
                    Content = JsonConvert.SerializeObject(new
                    {
                        ts = response.ts.Value,
                        status = incidentBuild.Status.ToString()
                    })
                });

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

        public string AlertPassing(MessageHandler alertHandler, Build incidentBuild, Build fixingBuild)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            //string secret = ContextPluginConfig.Config.First(r => r.Key == "Secret").Value.ToString();

            NameValueCollection data = new NameValueCollection();
            MessageConfiguration targetSlackConfig = null;

            if (!string.IsNullOrEmpty(alertHandler.User))
            {
                User user = _config.Users.Single(u => u.Key == alertHandler.User);
                targetSlackConfig = user.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(alertHandler.Group))
            {
                Group group = _config.Groups.Single(u => u.Key == alertHandler.Group);
                targetSlackConfig = group.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (targetSlackConfig == null)
                throw new Exception("alerthandler has neither user nor group");

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = config.SlackId;

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
            Job job = dataLayer.GetJobById(fixingBuild.JobId);

            // get message transaction
            string key = AlertKey(slackId, job.Id, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);

            if (storeItem == null)
                // no alert for this build was sent, ignore it
                return null;

            dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
            // build already marked as passing
            if ((string)storeItemPayload.status == fixingBuild.Status.ToString())
                return null;

            string message = $"Build fixed by #{fixingBuild.Identifier}, originally broken by #{incidentBuild.Identifier}.";
            dynamic attachment = new JObject();
            attachment.title = $"{job.Name} is working again";
            attachment.fallback = " ";
            attachment.color = "#007a5a";
            attachment.text = message;
            attachment["title_link"] = _urlHelper.Build(fixingBuild);

            var attachments = new JArray(1);
            attachments[0] = attachment;

            data["token"] = token;
            data["ts"] = (string)storeItemPayload.ts;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);

            dynamic response = ExecAPI("chat.update", data);

            if (response.ok.Value)
            {
                storeItem.Content = JsonConvert.SerializeObject(new
                {
                    ts = response.ts.Value,
                    status = fixingBuild.Status.ToString()
                });

                dataLayer.SaveStore(storeItem);

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

        private dynamic ExecAPI(string apiFragment, NameValueCollection data, string method = "POST")
        {
            if (data == null)
                data = new NameValueCollection();

            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
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

            throw new Exception($"Failed to get user channel for slack userid {slackUserId}: {response}");
        }

        public string TestHandler(MessageConfiguration messageConfiguration)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();
           
            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(messageConfiguration.RawJson);
            string slackId = config.SlackId;


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
            attachment.title = "Test message";
            attachment.text = message;
            attachment.title_link = _config.Address;

            var attachments = new JArray(1);
            attachments[0] = attachment;



            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);

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
                Console.WriteLine(channel);
        }

        public string DeleteAlert(object alertIdentifier)
        {
            return string.Empty;
        }

        #endregion
    }
}
