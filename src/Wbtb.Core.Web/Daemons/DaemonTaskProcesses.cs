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
            lock (Program.LockInstance)
                return _activePrrocesses.ContainsKey(task.Id);
        }

        public DaemonActiveProcess GetActive(DaemonTask task) 
        {
            lock (Program.LockInstance)
                if (_activePrrocesses.ContainsKey(task.Id))
                    return _activePrrocesses[task.Id];

            return null;
        }

        public DaemonBlockedProcess GetBlocked(DaemonTask task)
        {
            lock (Program.LockInstance)
                if (_blockedProcesses.ContainsKey(task.Id))
                    return _blockedProcesses[task.Id];

            return null;
        }

        public void MarkActive(DaemonTask task, IWebDaemon daemon, Build build, string description = "") 
        {
            lock (Program.LockInstance)
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
            lock (Program.LockInstance) 
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                {
                    _blockedProcesses[task.Id].ErrorCount++;
                    _blockedProcesses[task.Id].Reason = reason;
                }
                else
                    _blockedProcesses.Add(task.Id, new DaemonBlockedProcess
                    {
                        Task = task,
                        Reason = reason,
                        CreatedUtc = DateTime.UtcNow,
                        Daemon = daemon.GetType(),
                        Build = build,
                        BlockingProcesses = new DaemonTask[] { }
                    });

                if (_activePrrocesses.ContainsKey(task.Id))
                    _activePrrocesses.Remove(task.Id);
            }
        }

        public void MarkBlocked(DaemonTask task, IWebDaemon daemon, Build build, IEnumerable<DaemonTask> blocking) 
        {
            lock (Program.LockInstance)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                {
                    _blockedProcesses[task.Id].ErrorCount++;
                    _blockedProcesses[task.Id].BlockingProcesses = blocking;
                }
                else
                    _blockedProcesses.Add(task.Id, new DaemonBlockedProcess
                    {
                        Task = task,
                        Reason = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        Daemon = daemon.GetType(),
                        Build = build,
                        BlockingProcesses = blocking
                    });

                if (_activePrrocesses.ContainsKey(task.Id))
                    _activePrrocesses.Remove(task.Id);
            }
        }

        public void MarkBlocked(DaemonTask task, string reason)
        {
            lock (Program.LockInstance)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                {
                    _blockedProcesses[task.Id].ErrorCount++;
                    _blockedProcesses[task.Id].Reason = reason;
                }
                else
                    _blockedProcesses.Add(task.Id, new DaemonBlockedProcess
                    {
                        Task = task,
                        Reason = reason,
                        CreatedUtc = DateTime.UtcNow
                    });

                if (_activePrrocesses.ContainsKey(task.Id))
                    _activePrrocesses.Remove(task.Id);

            }
        }

        public void MarkDone(DaemonTask task)
        {
            lock (Program.LockInstance)
                if (_blockedProcesses.ContainsKey(task.Id)) 
                    _blockedProcesses.Remove(task.Id);

            lock (Program.LockInstance)
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

                    _activePrrocesses.Remove(task.Id);
                }
        }

        public bool HasActive(string key)
        {
            lock (Program.LockInstance)
                return _activePrrocesses.ContainsKey(key);
        }

        public bool HasActive(DaemonTask task)
        {
            lock (Program.LockInstance)
                return _activePrrocesses.ContainsKey(task.Id);
        }

        public void ClearActive(DaemonTask task) 
        {
            lock (Program.LockInstance)
                if (_activePrrocesses.ContainsKey(task.Id))
                    _activePrrocesses.Remove(task.Id);
        }

        public IEnumerable<DaemonDoneProcess> GetDone() 
        {
            lock (Program.LockInstance)
                return _doneProcesses.Where(d => d != null).OrderByDescending(d => d.DoneUTc);
        }

        public void ClearActive(string key)
        {
            lock (Program.LockInstance)
                if (_activePrrocesses.ContainsKey(key))
                    _activePrrocesses.Remove(key);
        }

        public IEnumerable<DaemonActiveProcess> GetAllActive() 
        {
            lock (Program.LockInstance)
                return _activePrrocesses.Values;
        }

        public IEnumerable<DaemonBlockedProcess> GetAllBlocked()
        {
            lock (Program.LockInstance)
                return _blockedProcesses.Values;
        }

        #endregion
    }
}
