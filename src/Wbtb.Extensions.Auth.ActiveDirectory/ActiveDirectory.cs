using System;
using Wbtb.Core.Common;
using System.DirectoryServices;
using System.Linq;

namespace Wbtb.Extensions.Auth.ActiveDirectory
{
    public class ActiveDirectory : Plugin, IAuthenticationPlugin 
    {

        #region FIELDS

        private readonly Config _config;

        #endregion

        #region CTORS

        public ActiveDirectory() 
        {
            _config = new Config();
        }
        #endregion

        public PluginInitResult InitializePlugin()
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

        public ReachAttemptResult AttemptReach()
        {
            string host = ContextPluginConfig.Config.First(r => r.Key == "Host").Value.ToString();
            string username = ContextPluginConfig.Config.First(r => r.Key == "User").Value.ToString();
            string password = ContextPluginConfig.Config.First(r => r.Key == "Password").Value.ToString();
            string bindProperty = ContextPluginConfig.Config.First(r => r.Key == "BindProperty").Value.ToString();


            using (var root = new DirectoryEntry($"LDAP://{host}", username, password))
            {
                using (var searcher = new DirectorySearcher(root))
                {
                    searcher.Filter = $"(&)";
                    try
                    {
                        var results = searcher.FindAll();
                        return new ReachAttemptResult { Reachable = true };
                    }
                    catch (Exception ex)
                    {
                        return new ReachAttemptResult { Exception = ex };
                    }
                }
            }
        }

        public void ListUsers()
        {
            string host = ContextPluginConfig.Config.First(r => r.Key == "Host").Value.ToString();
            string username = ContextPluginConfig.Config.First(r => r.Key == "User").Value.ToString();
            string password = ContextPluginConfig.Config.First(r => r.Key == "Password").Value.ToString();
            string bindProperty = ContextPluginConfig.Config.First(r => r.Key == "BindProperty").Value.ToString();


            using (var root = new DirectoryEntry($"LDAP://{host}", username, password))
            {
                using (var searcher = new DirectorySearcher(root))
                {
                    // looking for a specific user
                    searcher.Filter = $"(&(objectCategory=User)(objectClass=person))";

                    // FYI, non-null results means the user was found
                    var results = searcher.FindAll();
                    
                    foreach (SearchResult item in results)
                    {
                        
                        if (item.Properties[bindProperty] == null || item.Properties[bindProperty].Count == 0)
                        {
                            Console.WriteLine("----------------------------------------");
                            Console.WriteLine($"User without BindProperty \"{bindProperty}\".");

                            foreach (var prop in item.Properties.PropertyNames)
                                Console.WriteLine($"{prop}:{item.Properties[prop.ToString()][0]}");

                            continue;
                        }

                        Console.WriteLine($"Bind value:{item.Properties[bindProperty][0]}");
                    }
                }
            }
        }

        public AuthenticationResult RequestPasswordLogin(string username, string password)
        {
            string host = _config.Plugins.Single(r => r.Key == ContextPluginConfig.Key).Config.First(r => r.Key == "Host").Value.ToString();

            try
            {
                DirectoryEntry Ldap = new DirectoryEntry($"LDAP://{host}", username, password);
                // try to access guid to force login
                Guid uid = Ldap.Guid;

                return new AuthenticationResult
                {
                    User = new User { 
                        Key = uid.ToString()
                    },
                    Message = "Login succeeded!",
                    Success = true
                };
            }
            catch (DirectoryServicesCOMException ex)
            {       
                if (ex.Message == "The user name or password is incorrect.")
                    return new AuthenticationResult
                    {
                        Message = ex.Message,
                        Success = false
                    };

                throw ex;
            }
        }
    }
}
