using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class DaemonDoneProcess
    {
        public DateTime DoneUTc { get; set; }
        public string TaskId { get; set; }
        public string BuildId { get; set; }
        public string Daemon { get; set; }
    }
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



        const int _doneListSize = 5;
        int _currentDone = 0;
        DaemonDoneProcess[] _doneProcesses = new DaemonDoneProcess[_doneListSize];

        #endregion

        #region METHODS

        public bool IsActive(DaemonTask task) 
        {
            lock (_activePrrocesses)
            {
                return _activePrrocesses.ContainsKey(task.Id);
            }
        }

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

        public void MarkActive(DaemonTask task, IWebDaemon daemon, Build build, string description = "") 
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
                    Daemon = daemon.GetType(),
                    Build = build,
                    Task = task
                });
            }
        }

        public void MarkBlocked(DaemonTask task, IWebDaemon daemon, Build build, string reason = "")
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcess
                {
                    Task = task,
                    Reason = reason,
                    CreatedUtc = DateTime.UtcNow,
                    Daemon = daemon.GetType(),
                    Build = build,
                    BlockingProcesses = new DaemonTask[] { }
                });
            }
        }

        public void MarkBlocked(DaemonTask task, IWebDaemon daemon, Build build, IEnumerable<DaemonTask> blocking) 
        {
            lock (_blockedProcesses) 
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcess { 
                    Task = task, 
                    Reason = string.Empty, 
                    CreatedUtc = DateTime.UtcNow, 
                    Daemon = daemon.GetType(),
                    Build = build,
                    BlockingProcesses = blocking
                });
            }
        }

        public void MarkBlocked(DaemonTask task, string reason)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                    return;

                _blockedProcesses.Add(task.Id, new DaemonBlockedProcess { Task = task, Reason = reason, CreatedUtc = DateTime.UtcNow });
            }
        }

        public void MarkDone(DaemonTask task)
        {
            lock (_blockedProcesses)
            {
                if (_blockedProcesses.ContainsKey(task.Id)) 
                    _blockedProcesses.Remove(task.Id);
            }

            lock (_activePrrocesses)
            {
                if (_activePrrocesses.ContainsKey(task.Id)) 
                {
                    DaemonActiveProcess done = _activePrrocesses[task.Id];
                    _doneProcesses[_currentDone] = new DaemonDoneProcess
                    {
                        BuildId = done.Build.Id,
                        TaskId = done.Task.Id,
                        Daemon = done.Daemon.Name,
                        DoneUTc = DateTime.UtcNow
                    };

                    _currentDone++;
                    if (_currentDone >= _doneListSize)
                        _currentDone = 0;
                }

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

        public IEnumerable<DaemonDoneProcess> GetDone() 
        {
            return _doneProcesses.Where(d => d != null).OrderByDescending(d => d.DoneUTc);
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
