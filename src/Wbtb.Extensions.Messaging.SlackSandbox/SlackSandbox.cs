﻿using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.SlackSandbox
{
    public class SlackConfig
    {
        public string SlackId { get; set; }
        public bool IsGroup { get; set; }
    }

    public class SlackSandbox : Plugin, IMessaging
    {
        #region FIELDS

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Config _config;

        private readonly UrlHelper _urlHelper;

        private readonly PluginProvider _pluginProvider;

        #endregion

        #region CTORS

        public SlackSandbox(Config config, UrlHelper urlHelper, PluginProvider pluginProvider)
        {
            _config = config;
            _pluginProvider = pluginProvider;
            _urlHelper = urlHelper;
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
                dynamic forcedResponse = new { error = (string)null };
                dynamic response = ExecAPI("conversations.list", data, forcedResponse);

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
            if (string.IsNullOrEmpty(alertConfig.Plugin))
                throw new ConfigurationException("Slack detected alert with no \"Plugin\" value.");
        }

        private string AlertKey(string slackChannelId, string jobId, string incidentBuildId) 
        {
            return $"buildStatusAlert_slack_{slackChannelId}_job{jobId}_incident{incidentBuildId}";
        }

        public string AlertBreaking(AlertHandler alertHandler, Build incidentBuild)
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

            // check if alert has already been sent
            string key = AlertKey(slackId, job.Id, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);
            if (storeItem != null) 
                return null;

            string message = $"Build for {job.Name} broke at #{incidentBuild.Identifier}.";
            dynamic attachment = new JObject();
            attachment.fallback = " ";
            attachment.text = message;

            data["title_link"] = _urlHelper.Build(incidentBuild);
            data["channel"] = slackId;
            data["text"] = message;
            data["attachments"] = JsonConvert.SerializeObject(attachment);

            dynamic response = ExecAPI("chat.postMessage", data, new
            {
                ok = new
                {
                    Value = true
                },
                ts = new
                {
                    Value = "BreakingMessage-id-1234"
                }
            });

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

        public string AlertPassing(AlertHandler alertHandler, Build incidentBuild, Build fixingBuild)
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

            string message = $"Build for {job.Name} fixed by #{fixingBuild.Identifier}, originally broken by #{incidentBuild.Identifier}.";
            dynamic attachment = new JObject();
            attachment.fallback = " ";
            attachment.text = message;

            data["title_link"] = _urlHelper.Build(fixingBuild);
            data["token"] = token;
            data["ts"] = (string)storeItemPayload.ts;
            data["channel"] = slackId;
            data["text"] = message;
            data["attachments"] = JsonConvert.SerializeObject(attachment);

            dynamic response = ExecAPI("chat.update", data, new
            {
                ok = new
                {
                    Value = true
                },
                ts = new
                {
                    Value = "FixedMessage-id-1234"
                }
            });

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

        private dynamic ExecAPI(string apiFragment, NameValueCollection data, dynamic forcedResponse)
        {
            if (data == null)
                data = new NameValueCollection();

            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            string output = "";
            foreach (string key in data.AllKeys)
                output += $"{key} : {data.Get(key)}\n";

            // put plugin into sandbox mode, write message to file system
            string pluginDataDirectory = Path.Combine(_config.PluginDataPersistDirectory, this.ContextPluginConfig.Manifest.Key);
            Directory.CreateDirectory(pluginDataDirectory);
            File.WriteAllText(Path.Combine(pluginDataDirectory, $"{DateTime.UtcNow.Ticks}.txt"), output);

            return forcedResponse;
        }

        private string GetUserChannelId(string slackUserId)
        {
            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();
            data["token"] = token;
            data["users"] = slackUserId;

            dynamic channelLookup = new { 
                ok = new { 
                    Value = true
                },
                channel = new { 
                    id = new { 
                        Value = "user-channel-1234" 
                    }
                } 
            };

            dynamic response = ExecAPI("conversations.open", data, channelLookup);
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
            attachment.text = message;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = "test message";
            data["attachments"] = Convert.ToString(attachment);

            dynamic forceResponse = new { 
                ok = new { 
                    Value = true
                },
                ts = new { 
                    Value = "message-id-1234"
                }
            };
            dynamic response = ExecAPI("chat.postMessage", data, forceResponse);

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
            dynamic forcedResponse = new { 
                channels = new List<JToken> { JToken.FromObject(new { id = "123", name = "some channel" })}
            };

            dynamic response = ExecAPI("conversations.list", data, forcedResponse);
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