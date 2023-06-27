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

    public class DaemonActiveProcesses
    {
        IDictionary<string, DaemonActiveProcessItem> _processes = new Dictionary<string, DaemonActiveProcessItem>();

        public void Add(IWebDaemon daemon, string description) 
        {
            lock (_processes)
            {
                string name = daemon.GetType().Name;

                if (_processes.ContainsKey(name))
                    _processes.Remove(name);


                _processes.Add(name, new DaemonActiveProcessItem
                {
                    Description= description,
                    CreatedUtc = DateTime.UtcNow,
                    Daemon = name
                });
            }
        }

        public void Clear(IWebDaemon daemon) 
        {
            lock (_processes)
            {
                string name = daemon.GetType().Name;
                if (_processes.ContainsKey(name))
                    _processes.Remove(name);
            }
        }

        public IEnumerable<DaemonActiveProcessItem> GetCurrent() 
        { 
            return _processes.Values;
        }
    }
}
