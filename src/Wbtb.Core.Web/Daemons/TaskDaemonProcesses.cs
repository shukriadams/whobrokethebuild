using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

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

        public string TaskId { get; set; }

        public string Reason { get; set; }
    }

    public class TaskDaemonProcesses
    {
        /// <summary>
        /// key string is daemonName. This shows whatever process a given daemon is currently processing
        /// </summary>
        IDictionary<string, DaemonActiveProcessItem> _activePrrocesses = new Dictionary<string, DaemonActiveProcessItem>();

        IDictionary<string, DaemonBlockedProcessItem> _blockedProcesses = new Dictionary<string, DaemonBlockedProcessItem>();

        public void AddActive(IWebDaemon daemon, string description) 
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
        public void TaskBlocked(DaemonTask task, IWebDaemon daemon, IEnumerable<DaemonTask> blocking) 
        {
            lock (_blockedProcesses) 
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                string reason = $"Task {task.Id} blocked @ daemon {daemon.GetType().Name} by {blocking.Count()} preceeding tasks: {string.Join(", ", blocking.Select(b => b.Id))}.";

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcessItem { TaskId = task.Id, Reason = reason, CreatedUtc = DateTime.UtcNow });
            }
        }

        public void TaskBlocked(DaemonTask task, string reason)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcessItem { TaskId = task.Id, Reason = reason, CreatedUtc = DateTime.UtcNow });
            }
        }

        public void TaskDone(DaemonTask task)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    _blockedProcesses.Remove(task.Id);
            }
        }

        public void AddActive(string key, string description)
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

        public bool HasActive(string key)
        {
            lock (_activePrrocesses)
            {
                return _activePrrocesses.ContainsKey(key);
            }
        }

        public void ClearActive(IWebDaemon daemon) 
        {
            lock (_activePrrocesses)
            {
                string name = daemon.GetType().Name;
                if (_activePrrocesses.ContainsKey(name))
                    _activePrrocesses.Remove(name);
            }
        }

        public void ClearActive(string key)
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);
            }
        }

        public IEnumerable<DaemonActiveProcessItem> GetActive() 
        { 
            return _activePrrocesses.Values;
        }

        public IEnumerable<DaemonBlockedProcessItem> GetBlocked()
        {
            return _blockedProcesses.Values;
        }
    }
}
