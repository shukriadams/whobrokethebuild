﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Wbtb.Core.Common;
using Microsoft.Extensions.Logging;

namespace Wbtb.Extensions.Messaging.Slack
{
    public class Slack : Plugin, IMessagingPlugin
    {
        #region FIELDS

        static readonly string[] allowedTargetTypes = new string[] { "user", "group" };

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly UrlHelper _urlHelper;

        private readonly ILogger _log;

        #endregion

        #region CTORS

        public Slack(Configuration config, UrlHelper urlHelper, PluginProvider pluginProvider, ILogger log) 
        {
            _config = config;
            _urlHelper = urlHelper;
            _pluginProvider = pluginProvider;
            _log = log;
        }

        #endregion

        #region PLUMBING METHODS

        private dynamic ExecAPI(string apiFragment, NameValueCollection data, string method = "POST")
        {
            if (data == null)
                data = new NameValueCollection();

            string token = this.ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();
            WebClient client = new WebClient();
            string jsonResponse = Encoding.UTF8.GetString(client.UploadValues($"https://slack.com/api/{apiFragment}", method, data));
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
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
            return $"incident{incidentBuildId}_job{jobId}_buildStatusAlert_slack_{slackChannelId}";
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
                summary = $"{mutationReport.Summary}. Build {incidentBuild.Key}";
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
            string replies = string.Empty;
            if (storeItem != null) 
            {
                dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
                ts = (string)storeItemPayload.ts;
                replies = (string)storeItemPayload.repies;
                if (replies == null)
                    replies = string.Empty;
            }

            string mentionsFlattened = string.Empty;
            mentions = mentions.Distinct().ToList();
            if (mentions.Any())
                mentionsFlattened = $"\n{string.Join(" ", mentions)}";

            dynamic attachment = new JObject();
            attachment.fallback = " ";
            attachment.color = isMutation ? "#f58742" : "#D92424";
            attachment.text = $"```{description}```{mentionsFlattened}";
            attachment.title_link = _urlHelper.Build(incidentBuild);
            attachment.title = $"{(isMutation ? "Error changed" : job.Name)} - {summary}";

            var attachments = new JArray(1);
            attachments[0] = attachment;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);
            if (!string.IsNullOrEmpty(ts))
                data["thread_ts"] = ts;

            dynamic response = response = ExecAPI("chat.postMessage", data);

            if (response.ok.Value)
            {
                if (string.IsNullOrEmpty(ts))
                    ts = ts = response.ts.Value;
                else
                    replies += $",{response.ts.Value}";

                // store message info and proof of sending
                dataLayer.DeleteStoreItemWithKey(key);
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key,
                    Content = JsonConvert.SerializeObject(new
                    {
                        ts = ts, // this ts is always the original incident post ts
                        replies = replies,
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

            if (!incidentBuild.EndedUtc.HasValue)
                return "incident build not complete";

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(incidentBuild.JobId);

            int alertMaxLength = 600;
            if (ContextPluginConfig.Config.Any(r => r.Key == "AlertMaxLength"))
                int.TryParse(ContextPluginConfig.Config.First(r => r.Key == "AlertMaxLength").Value.ToString(), out alertMaxLength);

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

            string summary = string.Empty;
            string description = string.Empty;

            TimeSpan downtime = DateTime.UtcNow - incidentBuild.EndedUtc.Value;
            summary = $" This is still broken since build {incidentBuild.Key}, and has been down for {downtime.ToHumanString()}";
            description = "Click link for more info";

            string key = AlertKey(slackId, job.Key, incidentBuild.IncidentBuildId);
            StoreItem storeItem = dataLayer.GetStoreItemByKey(key);
            string ts = string.Empty;
            string replies = string.Empty;
            if (storeItem != null)
            {
                dynamic storeItemPayload = Newtonsoft.Json.JsonConvert.DeserializeObject(storeItem.Content);
                replies = (string)storeItemPayload.repies;
                if (replies == null)
                    replies = string.Empty;
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
            attachment.title = $"{job.Name} - {summary}";

            var attachments = new JArray(1);
            attachments[0] = attachment;

            data["token"] = token;
            data["channel"] = slackId;
            data["text"] = " ";
            data["attachments"] = Convert.ToString(attachments);

            dynamic response = response = ExecAPI("chat.postMessage", data);

            if (response.ok.Value)
            {
                replies += $",{response.ts.Value}";

                // store message info and proof of sending
                dataLayer.DeleteStoreItemWithKey(key);
                dataLayer.SaveStore(new StoreItem
                {
                    Plugin = this.ContextPluginConfig.Manifest.Key,
                    Key = key,
                    Content = JsonConvert.SerializeObject(new
                    {
                        ts = ts, // this ts is always the original incident post ts
                        replies = replies,
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
                return "failed, check logs";
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

            dynamic response = ExecAPI("chat.update", data);

            // todo : check if message is still updated even if ts match doesn't occur.
            if (response.ok.Value)
            {
                storeItem.Content = JsonConvert.SerializeObject(new {
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

            dynamic response = ExecAPI("conversations.open", data);
            if (response.ok.Value == true)
                return response.channel.id.Value;

            throw new Exception($"Failed to get user channel for slack userid {slackUserId}: {Convert.ToString(response)}");
        }


        string IMessagingPlugin.TestHandler(MessageConfiguration messageConfiguration)
        {
            string token = ContextPluginConfig.Config.First(r => r.Key == "Token").Value.ToString();

            NameValueCollection data = new NameValueCollection();
           
            SlackConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<SlackConfig>(messageConfiguration.RawJson);

            // if user, we need to get user channel id from user slack id, and post to this
            string slackId = config.SlackId;
            if (!config.IsGroup)
                slackId = this.GetUserChannelId(slackId);

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
                _log.LogError($"Error posting to slack: {Convert.ToString(response)}, settings are {messageConfiguration.RawJson}");
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
                ConsoleHelper.WriteLine(channel);
        }

        string IMessagingPlugin.DeleteAlert(object alertIdentifier)
        {
            return string.Empty;
        }

        #endregion
    }
}
