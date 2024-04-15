using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Auth.ActiveDirectorySandbox
{

    public class ActiveDirectorySandbox : Plugin, IAuthenticationPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing item \"Host\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Base"))
                throw new ConfigurationException("Missing item \"Base\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "User"))
                throw new ConfigurationException("Missing item \"User\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Password"))
                throw new ConfigurationException("Missing item \"Password\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "BindProperty"))
                throw new ConfigurationException("Missing item \"BindProperty\"");

            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        ReachAttemptResult IReachable.AttemptReach()
        {
            return new ReachAttemptResult { Reachable = true };
        }

        private void ListUsers()
        {
            string host = ContextPluginConfig.Config.First(r => r.Key == "Host").Value.ToString();
            string username = ContextPluginConfig.Config.First(r => r.Key == "User").Value.ToString();
            string password = ContextPluginConfig.Config.First(r => r.Key == "Password").Value.ToString();
            string bindProperty = ContextPluginConfig.Config.First(r => r.Key == "BindProperty").Value.ToString();

            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), "JSON.users.json");
            IEnumerable<ADUser> users = JsonConvert.DeserializeObject<IEnumerable<ADUser>>(rawJson);
            foreach(ADUser user in users)
                ConsoleHelper.WriteLine(user.Name);

        }

        AuthenticationResult IAuthenticationPlugin.RequestPasswordLogin(string username, string password)
        {
            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), "JSON.users.json");
            IEnumerable<ADUser> users = JsonConvert.DeserializeObject<IEnumerable<ADUser>>(rawJson);

            ADUser user = users.SingleOrDefault(u => u.Name == username);
            if (user == null)
                return new AuthenticationResult
                {
                    Message = "Invalid user",
                    Success = false
                };

            if (user.Password != password)
                return new AuthenticationResult
                {
                    Message = "Invalid password",
                    Success = false
                };
            
            return new AuthenticationResult
            {
                User = new User
                {
                    Key = user.Uid
                },
                Message = "Login succeeded!",
                Success = true
            };
        }
    }
}
