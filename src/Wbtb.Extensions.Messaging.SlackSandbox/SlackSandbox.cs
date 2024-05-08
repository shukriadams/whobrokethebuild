using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using Wbtb.Core.Common;
using Microsoft.Extensions.Logging;

namespace Wbtb.Extensions.Messaging.SlackSandbox
{
    public class SlackConfig
    {
        public string SlackId { get; set; }
        public bool IsGroup { get; set; }
    }

    public class SlackSandbox : Plugin, IMessagingPlugin
    {
        #region FIELDS

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Configuration _config;

        private readonly UrlHelper _urlHelper;

        private readonly PluginProvider _pluginProvider;

        private readonly ILogger _log;

        #endregion

        #region CTORS

        public SlackSandbox(Configuration config, UrlHelper urlHelper, PluginProvider pluginProvider, ILogger log)
        {
            _config = config;
            _pluginProvider = pluginProvider;
            _urlHelper = urlHelper;
            _log = log;
        }

        #endregion

        #region SANDBOX METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiFragment"></param>
        /// <param name="data"></param>
        /// <param name="forcedResponse"></param>
        /// <returns></returns>
        private dynamic ExecAPI(string apiFragment, NameValueCollection data, dynamic forcedResponse)
        {
            if (data == null)
                data = new NameValueCollection();

            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            string output = "";
            foreach (string key in data.AllKeys)
                output += $"{key} : {data.Get(key)}\n";

            if (forcedResponse != null)
                output += $"Forced response:\n{Convert.ToString(forcedResponse)}";

            // put plugin into sandbox mode, write message to file system
            string pluginDataDirectory = Path.Combine(_config.PluginDataPersistDirectory, this.ContextPluginConfig.Manifest.Key);
            Directory.CreateDirectory(pluginDataDirectory);
            File.WriteAllText(Path.Combine(pluginDataDirectory, $"{apiFragment}-{DateTime.UtcNow.Ticks}.txt"), output);

            return forcedResponse;
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

        PluginInitResult IPlugin.InitializePlugin()
        {
            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Token"))
                throw new ConfigurationException("Missing item \"Token\"");

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

        string IMessagingPlugin.AlertBreaking(string user, string group, Build incidentBuild, bool isMutation, bool force)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            bool mentionUsers = Configuration.GetConfigValue(ContextPluginConfig.Config, "MentionUsersInGroupPosts", "false").ToLower() == "true";
            bool mute = Configuration.GetConfigValue(ContextPluginConfig.Config, "Mute", "false").ToLower() == "true";
            if (mute)
                return "muted";

            int alertMaxLength = 600;
            if (ContextPluginConfig.Config.Any(r => r.Key == "AlertMaxLength"))
                int.TryParse(ContextPluginConfig.Config.First(r => r.Key == "AlertMaxLength").Value.ToString(), out alertMaxLength);

            NameValueCollection data = new NameValueCollection();
            MessageConfiguration targetSlackConfig = null;
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(incidentBuild.JobId);

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
                throw new Exception($"Job {job.Key} specified user:{user} and group:{group}, but neither are valid");

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = config.SlackId;

            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
                slackId = this.GetUserChannelId(slackId);

            IEnumerable<BuildInvolvement> buildInvolvements = dataLayer.GetBuildInvolvementsByBuild(incidentBuild.Id);

            // get slack id's of users confirmed involved in break
            List<string> mentions = new List<string>();
            if (mentionUsers)
            {
                foreach (BuildInvolvement bi in buildInvolvements.Where(bi => bi.BlameScore == 100 && !string.IsNullOrEmpty(bi.MappedUserId)))
                {
                    User mappedUser = dataLayer.GetUserById(bi.MappedUserId);
                    MessageConfiguration messageConfig = mappedUser.Message.SingleOrDefault(m => m.Plugin == this.ContextPluginConfig.Key);
                    if (messageConfig == null)
                        continue;

                    SlackConfig slackConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(messageConfig.RawJson);
                    mentions.Add($"<@{slackConfig.SlackId}>");
                }
            }

            MutationReport mutationReport = dataLayer.GetMutationReportByBuild(incidentBuild.Id);
            string summary = string.Empty;
            string description = string.Empty;

            if (mutationReport != null)
            {
                summary = mutationReport.Summary;
                description = mutationReport.Description;
            }
            else
            {
                summary = $" Broke at build {incidentBuild.Key}";
                description = "No error cause determined. Please check build log for details.";
            }

            if (description.Length > alertMaxLength)
                description = $"{description.Substring(0, alertMaxLength)}\n...\n(truncated, click link for more)";

            string key = AlertKey(slackId, job.Key, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);
            string ts = string.Empty;
            if (storeItem != null)
            {
                dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
                ts = (string)storeItemPayload.ts;
            }

            string mentionsFlattened = string.Empty;
            mentions = mentions.Distinct().ToList();
            if (mentions.Any())
                mentionsFlattened = $"\n{string.Join(" ", mentions)}";

            dynamic attachment = new JObject();
            attachment.fallback = " ";
            attachment.color = "#D92424";
            attachment.text = $"```{description}```{mentionsFlattened}";
            attachment.title_link = _urlHelper.Build(incidentBuild);
            // no need to show name if mutation, mutation will be appended to thread
            attachment.title = $"{(isMutation ? "Error changed" : job.Name)} - {summary}";

            var attachments = new JArray(1);
            attachments[0] = attachment;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);
            if (!string.IsNullOrEmpty(ts))
                data["thread_ts"] = ts;

            dynamic response = ExecAPI("chat.postMessage", data, new
            {
                ok = new
                {
                    Value = true
                },
                ts = new
                {
                    Value = DateTime.UtcNow.Ticks.ToString()
                }
            });

            // check if alert has already been sent
            if (response.ok.Value)
            {
                // store message info and proof of sending
                dataLayer.DeleteStoreItemWithKey(key);
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key,
                    Content = JsonConvert.SerializeObject(new
                    {
                        ts = response.ts.Value,
                        failingDateUtc = DateTime.UtcNow,
                        failingBuildId = incidentBuild.Id,
                        status = incidentBuild.Status.ToString()
                    })
                });

                return response.ts.Value;
            }
            else
            {
                // log error
                // mark message sent as failed, somwhere
                _log.LogError($"Error posting to slack : {Convert.ToString(response)}");
                return null;
            }
        }

        string IMessagingPlugin.RemindBreaking(string user, string group, Build incidentBuild, bool force)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            bool mentionUsers = Configuration.GetConfigValue(ContextPluginConfig.Config, "MentionUsersInGroupPosts", "false").ToLower() == "true";
            bool mute = Configuration.GetConfigValue(ContextPluginConfig.Config, "Mute", "false").ToLower() == "true";
            if (mute)
                return "muted";

            int alertMaxLength = 600;
            if (ContextPluginConfig.Config.Any(r => r.Key == "AlertMaxLength"))
                int.TryParse(ContextPluginConfig.Config.First(r => r.Key == "AlertMaxLength").Value.ToString(), out alertMaxLength);

            NameValueCollection data = new NameValueCollection();
            MessageConfiguration targetSlackConfig = null;
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(incidentBuild.JobId);

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
                throw new Exception($"Job {job.Key} specified user:{user} and group:{group}, but neither are valid");

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = config.SlackId;

            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
                slackId = this.GetUserChannelId(slackId);

            IEnumerable<BuildInvolvement> buildInvolvements = dataLayer.GetBuildInvolvementsByBuild(incidentBuild.Id);

            // get slack id's of users confirmed involved in break
            List<string> mentions = new List<string>();
            if (mentionUsers)
            {
                foreach (BuildInvolvement bi in buildInvolvements.Where(bi => bi.BlameScore == 100 && !string.IsNullOrEmpty(bi.MappedUserId)))
                {
                    User mappedUser = dataLayer.GetUserById(bi.MappedUserId);
                    MessageConfiguration messageConfig = mappedUser.Message.SingleOrDefault(m => m.Plugin == this.ContextPluginConfig.Key);
                    if (messageConfig == null)
                        continue;

                    SlackConfig slackConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(messageConfig.RawJson);
                    mentions.Add($"<@{slackConfig.SlackId}>");
                }
            }

            MutationReport mutationReport = dataLayer.GetMutationReportByBuild(incidentBuild.Id);
            string summary = string.Empty;
            string description = string.Empty;

            if (mutationReport != null)
            {
                summary = mutationReport.Summary;
                description = mutationReport.Description;
            }
            else
            {
                summary = $" Broke at build {incidentBuild.Key}";
                description = "No error cause determined. Please check build log for details.";
            }

            if (description.Length > alertMaxLength)
                description = $"{description.Substring(0, alertMaxLength)}\n...\n(truncated, click link for more)";

            string key = AlertKey(slackId, job.Key, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);
            string ts = string.Empty;
            if (storeItem != null)
            {
                dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
                ts = (string)storeItemPayload.ts;
            }

            string mentionsFlattened = string.Empty;
            mentions = mentions.Distinct().ToList();
            if (mentions.Any())
                mentionsFlattened = $"\n{string.Join(" ", mentions)}";

            dynamic attachment = new JObject();
            attachment.fallback = " ";
            attachment.color = "#D92424";
            attachment.text = $"```{description}```{mentionsFlattened}";
            attachment.title_link = _urlHelper.Build(incidentBuild);
            // no need to show name if mutation, mutation will be appended to thread
            attachment.title = $"{job.Name} - {summary}";

            var attachments = new JArray(1);
            attachments[0] = attachment;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);
            if (!string.IsNullOrEmpty(ts))
                data["thread_ts"] = ts;

            dynamic response = ExecAPI("chat.postMessage", data, new
            {
                ok = new
                {
                    Value = true
                },
                ts = new
                {
                    Value = DateTime.UtcNow.Ticks.ToString()
                }
            });

            // check if alert has already been sent
            if (response.ok.Value)
            {
                // store message info and proof of sending
                dataLayer.DeleteStoreItemWithKey(key);
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key,
                    Content = JsonConvert.SerializeObject(new
                    {
                        ts = response.ts.Value,
                        failingDateUtc = DateTime.UtcNow,
                        failingBuildId = incidentBuild.Id,
                        status = incidentBuild.Status.ToString()
                    })
                });

                return response.ts.Value;
            }
            else
            {
                // log error
                // mark message sent as failed, somwhere
                _log.LogError($"Error posting to slack : {Convert.ToString(response)}");
                return null;
            }
        }


        string IMessagingPlugin.AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            bool mute = Configuration.GetConfigValue(ContextPluginConfig.Config, "Mute", "false").ToLower() == "true";
            if (mute)
                return "muted";

            NameValueCollection data = new NameValueCollection();
            MessageConfiguration targetSlackConfig = null;

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(fixingBuild.JobId);

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
                throw new Exception($"Job {job.Key} specified user:{user} and group:{group}, but neither are valid");

            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(targetSlackConfig.RawJson);
            string slackId = config.SlackId;

            // if user, we need to get user channel id from user slack id, and post to this
            if (!config.IsGroup)
                slackId = this.GetUserChannelId(slackId);

            // get message transaction
            string key = AlertKey(slackId, job.Key, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);

            IEnumerable<Build> buildsInIncident = dataLayer.GetBuildsByIncident(incidentBuild.IncidentBuildId);
            string ts = string.Empty;
            string failingDateUtc = string.Empty;
            string status = string.Empty;
            string failingBuildId = string.Empty;
            Incident incident = dataLayer.GetIncident(incidentBuild.Id);

            if (storeItem == null)
            {
                storeItem = new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key
                };
            }
            else
            {
                dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
                ts = (string)storeItemPayload.ts;
                failingDateUtc = (string)storeItemPayload.failingDateUtc;
                status = (string)storeItemPayload.Status;
                failingBuildId = (string)storeItemPayload.failingBuildId;
            }

            string message = $"Build fixed by #{fixingBuild.Key}, originally broken by #{incidentBuild.Key}.";
            if (incident != null)
            {
                if (incident.Duration.HasValue)
                    message += $"Broken for {incident.Duration.ToHumanString()}.";

                if (incident.BuildsInIncident > 0)
                    message += $"Spanned {incident.BuildsInIncident} build attempts.";
            }

            dynamic attachment = new JObject();
            attachment.title = $"{job.Name} is working again";
            attachment.fallback = " ";
            attachment.color = "#007a5a";
            attachment.text = message;
            attachment["title_link"] = _urlHelper.Build(fixingBuild);

            JArray attachments = new JArray(1);
            attachments[0] = attachment;
            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);
            if (ts != null)
                data["ts"] = ts;

            dynamic response = ExecAPI("chat.update", data, new
            {
                ok = new
                {
                    Value = true
                },
                ts = new
                {
                    Value = DateTime.UtcNow.Ticks.ToString()
                }
            });

            if (response.ok.Value)
            {
                storeItem.Content = JsonConvert.SerializeObject(new
                {
                    ts,
                    failingDateUtc,
                    status,
                    failingBuildId,
                    passingts = response.ts.Value,
                    passingBuildId = fixingBuild.Id,
                    passingDateUtc = DateTime.UtcNow
                });

                dataLayer.SaveStore(storeItem);

                // message sent
                return response.ts.Value;
            }
            else
            {
                // log error
                // mark message sent as failed, somwhere
                _log.LogError($"Error posting to slack : {Convert.ToString(response)}");
                return Convert.ToString(response);
            }
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
                    ConsoleHelper.WriteLine(ex);
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
                ConsoleHelper.WriteLine(response);
                return null;
            }
        }

        private void ListChannels()
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
                ConsoleHelper.WriteLine(channel);
        }

        string IMessagingPlugin.DeleteAlert(object alertIdentifier)
        {
            return string.Empty;
        }

        #endregion
    }
}
