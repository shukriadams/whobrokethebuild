using System;
using System.Collections.Generic;

namespace Wbtb.Core.Web
{
    public class DaemonActiveProcessItem 
    {
        public DateTime CreatedUtc { get; set; }
        
        public string Description { get; set; }

        public string Daemon { get; set; }
    }

    public class DaemonBlockedProcessItem
    {
        public DateTime CreatedUtc { get; set; }

        public string BlockedBuildId { get; set; }

        public string BlockingBuildId { get; set; }

        public int TaskGroup { get; set; }

        public string Daemon { get; set; }
    }

    public class DaemonActiveProcesses
    {
        /// <summary>
        /// key string is daemonName. This shows whatever process a given daemon is currently processing
        /// </summary>
        IDictionary<string, DaemonActiveProcessItem> _activePrrocesses = new Dictionary<string, DaemonActiveProcessItem>();

        public void Add(IWebDaemon daemon, string description) 
        {
            lock (_activePrrocesses)
            {
                string daemonName = daemon.GetType().Name;

                if (_activePrrocesses.ContainsKey(daemonName))
                    _activePrrocesses.Remove(daemonName);


                _activePrrocesses.Add(daemonName, new DaemonActiveProcessItem
                {
                    Description= description,
                    CreatedUtc = DateTime.UtcNow,
                    Daemon = daemonName
                });
            }
        }

        public void Add(string key, string description)
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);


                _activePrrocesses.Add(key, new DaemonActiveProcessItem
                {
                    Description = description,
                    CreatedUtc = DateTime.UtcNow,
                    Daemon = key
                });
            }
        }

        public bool Has(string key)
        {
            lock (_activePrrocesses)
            {
                return _activePrrocesses.ContainsKey(key);
            }
        }

        public void Clear(IWebDaemon daemon) 
        {
            lock (_activePrrocesses)
            {
                string name = daemon.GetType().Name;
                if (_activePrrocesses.ContainsKey(name))
                    _activePrrocesses.Remove(name);
            }
        }

        public void Clear(string key)
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);
            }
        }

        public IEnumerable<DaemonActiveProcessItem> GetCurrent() 
        { 
            return _activePrrocesses.Values;
        }
    }
}
