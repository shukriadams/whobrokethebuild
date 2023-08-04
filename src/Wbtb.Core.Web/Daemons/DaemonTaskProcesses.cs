using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// In-memory store of current daemon processes. Used to track daemon activity, performance and blockages.
    /// </summary>
    public class DaemonTaskProcesses
    {
        #region FIELDS

        /// <summary>
        /// key string is taskid. This shows whatever process a given daemon is currently processing
        /// </summary>
        IDictionary<string, DaemonActiveProcess> _activePrrocesses = new Dictionary<string, DaemonActiveProcess>();

        /// <summary>
        /// Key string is task id.
        /// </summary>
        IDictionary<string, DaemonBlockedProcess> _blockedProcesses = new Dictionary<string, DaemonBlockedProcess>();

        #endregion

        #region METHODS

        public DaemonActiveProcess GetActive(DaemonTask task) 
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(task.Id))
                    return _activePrrocesses[task.Id];
                return null;
            }
        }

        public DaemonBlockedProcess GetBlocked(DaemonTask task)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return _blockedProcesses[task.Id];
                return null;
            }
        }

        public void MarkActive(DaemonTask task, string description) 
        {
            lock (_activePrrocesses)
            {
                string key = task.Id;

                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);

                _activePrrocesses.Add(key, new DaemonActiveProcess
                {
                    Description= description,
                    CreatedUtc = DateTime.UtcNow,
                    Daemon = key
                });
            }
        }

        public void MarkBlocked(DaemonTask task, IWebDaemon daemon, IEnumerable<DaemonTask> blocking) 
        {
            lock (_blockedProcesses) 
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                string reason = $"Task {task.Id} blocked @ daemon {daemon.GetType().Name} by {blocking.Count()} preceeding tasks: {string.Join(", ", blocking.Select(b => b.Id))}.";

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcess { TaskId = task.Id, Reason = reason, CreatedUtc = DateTime.UtcNow, BlockingProcesses = blocking.Select(b => b.Id) });
            }
        }

        public void MarkBlocked(DaemonTask task, string reason)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcess { TaskId = task.Id, Reason = reason, CreatedUtc = DateTime.UtcNow });
            }
        }

        public void MarkDone(DaemonTask task)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    _blockedProcesses.Remove(task.Id);
            }
        }

        public void MarkActive(string key, string description)
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);


                _activePrrocesses.Add(key, new DaemonActiveProcess
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

        public bool HasActive(DaemonTask task)
        {
            lock (_activePrrocesses)
            {
                return _activePrrocesses.ContainsKey(task.Id);
            }
        }

        public void ClearActive(DaemonTask task) 
        {
            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(task.Id))
                    _activePrrocesses.Remove(task.Id);
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

        public IEnumerable<DaemonActiveProcess> GetAllActive() 
        { 
            return _activePrrocesses.Values;
        }

        public IEnumerable<DaemonBlockedProcess> GetAllBlocked()
        {
            return _blockedProcesses.Values;
        }

        #endregion
    }
}
