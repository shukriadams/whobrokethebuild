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

    public class Slack : Plugin, IMessagingPlugin
    {
        #region FIELDS

        static readonly int MAX_ALERT_LENGTH = 600;

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly Cache _cache;

        private readonly UrlHelper _urlHelper;

        #endregion

        #region CTORS

        public Slack(Configuration config, Cache cache, UrlHelper urlHelper, PluginProvider pluginProvider) 
        {
            _config = config;
            _urlHelper = urlHelper;
            _pluginProvider = pluginProvider;
            _cache = cache;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Attempt to reach slack to ensure config works
        /// </summary>
        /// <returns></returns>
        ReachAttemptResult IReachable.AttemptReach()
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

        PluginInitResult IPlugin.InitializePlugin()
        {
            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Token"))
                throw new ConfigurationException("Missing Config item \"Token\"");

            if (string.IsNullOrEmpty(_config.Address))
                throw new ConfigurationException("Slack alerts require WBTB Core \"Address\" to be set, these are used to generate links.");

            return new PluginInitResult
            {
                Success = true,
                SessionId = Guid.NewGuid().ToString()
            };
        }

        void IMessagingPlugin.ValidateAlertConfig(MessageConfiguration alertConfig)
        {
            if (string.IsNullOrEmpty(alertConfig.Plugin))
                throw new ConfigurationException("Slack detected alert with no \"Plugin\" value.");
        }

        private string AlertKey(string slackChannelId, string jobId, string incidentBuildId)
        {
            return $"buildStatusAlert_slack_{slackChannelId}_job{jobId}_incident{incidentBuildId}";
        }


        string IMessagingPlugin.AlertBreaking(string user, string group, Build incidentBuild, bool force)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();

            MessageConfiguration targetSlackConfig = null;
             
            if (!string.IsNullOrEmpty(user))
            {
                User userData = _config.Users.Single(u => u.Key == user);
                targetSlackConfig = userData.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(group))
            {
                Group groupData = _config.Groups.Single(u => u.Key == group);
                targetSlackConfig = groupData.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
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

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(incidentBuild.JobId);

            // check if alert has already been sent
            string key = AlertKey(slackId, job.Id, incidentBuild.IncidentBuildId);
            if (!force && _cache.Get(this, key) != null)
                return null;

            IncidentReport incidentReport = dataLayer.GetIncidentReportByMutation(incidentBuild.Id);
            string summary = string.Empty;
            string description = string.Empty;

            if (incidentReport != null)
            {
                summary = incidentReport.Summary;
                description = incidentReport.Description;
            }
            else 
            {
                summary = $" Broke at build {incidentBuild.Identifier}";
                description = "No error cause determined. Please check build log for details.";

                // in absence of proof of break, present all existing log parse results
                IEnumerable<BuildLogParseResult> logParseResults = dataLayer.GetBuildLogParseResultsByBuildId(incidentBuild.Id);
                if (logParseResults.Where(r => !string.IsNullOrEmpty(r.ParsedContent)).Any())
                {
                    description = "No error cause, log parse returned :\n";

                    foreach (BuildLogParseResult result in logParseResults.Where(r => !string.IsNullOrEmpty(r.ParsedContent))) 
                    {
                        description += "\n";

                        ILogParserPlugin logparser = _pluginProvider.GetByKey(result.LogParserPlugin) as ILogParserPlugin;
                        if (logparser == null)
                            description += result.LogParserPlugin;
                        else 
                            description += logparser.ContextPluginConfig.Key;

                        description += "\n---------------------------------------\n";
                        ParsedBuildLogText parsedText = BuildLogTextParser.Parse(result.ParsedContent);
                        if (parsedText != null) 
                        {
                            foreach(var item in parsedText.Items) 
                            {
                                foreach (var item2 in item.Items)
                                    description += $"{item2.Content} ";

                                description += "\n";
                            }
                        }

                    }
                }
            }

            int maxAlertLength = 600; // move this to config
            if (description.Length > maxAlertLength)
                description = $"{description.Substring(0, maxAlertLength)}\n...\n(truncated, click link for more)";

            dynamic attachment = new JObject();
            attachment.title = $"{job.Name} - {summary}";
            attachment.fallback = " ";
            attachment.color = "#D92424";
            attachment.text = $"```{description}```";
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
                // write 
                _cache.Write(this, key, "sent");

                // store message info and proof of sending
                dataLayer.DeleteStoreItemWithKey(key);
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key,
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

        string IMessagingPlugin.AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();
            MessageConfiguration targetSlackConfig = null;

            if (!string.IsNullOrEmpty(user))
            {
                User userData = _config.Users.Single(u => u.Key == user);
                targetSlackConfig = userData.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
            }

            if (!string.IsNullOrEmpty(group))
            {
                Group groupData = _config.Groups.Single(u => u.Key == group);
                targetSlackConfig = groupData.Message.First(c => c.Plugin == this.ContextPluginConfig.Key);
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

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(fixingBuild.JobId);

            // get message transaction
            string key = AlertKey(slackId, job.Id, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);

            // no alert for this build was sent, ignore it
            if (storeItem == null)
                return "no fail alert to update";

            dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
 
            string message = $"Build fixed by #{fixingBuild.Identifier}, originally broken by #{incidentBuild.Identifier}.";
            dynamic attachment = new JObject();
            attachment.title = $"{job.Name} is working again";
            attachment.fallback = " ";
            attachment.color = "#007a5a";
            attachment.text = message;
            attachment["title_link"] = _urlHelper.Build(fixingBuild);

            JArray attachments = new JArray(1);
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
                return response;
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

        string IMessagingPlugin.TestHandler(MessageConfiguration messageConfiguration)
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

            JArray attachments = new JArray(1);
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

        private void ListChannels()
        {
            NameValueCollection data = new NameValueCollection();
            data["token"] = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            // list channels to ensure connection works
            dynamic response = ExecAPI("conversations.list", data);
            IEnumerable<JToken> channels = Enumerable.ToList(response.channels);
            foreach (JToken channel in channels)
                Console.WriteLine(channel);
        }

        string IMessagingPlugin.DeleteAlert(object alertIdentifier)
        {
            return string.Empty;
        }

        #endregion
    }
}
