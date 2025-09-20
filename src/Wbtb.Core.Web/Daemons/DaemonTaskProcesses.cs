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
        IDictionary<string, DaemonActiveProcess> _activeProcesses = new Dictionary<string, DaemonActiveProcess>();

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
            lock (ProgramStart.LockInstance)
                return _activeProcesses.ContainsKey(task.Id);
        }

        public DaemonActiveProcess GetActive(DaemonTask task) 
        {
            lock (ProgramStart.LockInstance)
                if (_activeProcesses.ContainsKey(task.Id))
                    return _activeProcesses[task.Id];

            return null;
        }

        public DaemonBlockedProcess GetBlocked(DaemonTask task)
        {
            lock (ProgramStart.LockInstance)
                if (_blockedProcesses.ContainsKey(task.Id))
                    return _blockedProcesses[task.Id];

            return null;
        }

        public void MarkActive(DaemonTask task, IWebDaemon daemon, Build build, string description = "") 
        {
            lock (ProgramStart.LockInstance)
            {
                string key = task.Id;

                if (_activeProcesses.ContainsKey(key))
                    _activeProcesses.Remove(key);

                _activeProcesses.Add(key, new DaemonActiveProcess
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
            lock (ProgramStart.LockInstance) 
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                {
                    _blockedProcesses[task.Id].BlockedCount++;
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

                if (_activeProcesses.ContainsKey(task.Id))
                    _activeProcesses.Remove(task.Id);
            }
        }

        public void MarkBlocked(DaemonTask task, IWebDaemon daemon, Build build, IEnumerable<DaemonTask> blocking) 
        {
            lock (ProgramStart.LockInstance)
            {
                if (_blockedProcesses.ContainsKey(task.Id))
                {
                    _blockedProcesses[task.Id].BlockedCount++;
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

                if (_activeProcesses.ContainsKey(task.Id))
                    _activeProcesses.Remove(task.Id);
            }
        }

        public void MarkDone(DaemonTask task)
        {
            lock (ProgramStart.LockInstance)
                if (_blockedProcesses.ContainsKey(task.Id)) 
                    _blockedProcesses.Remove(task.Id);

            lock (ProgramStart.LockInstance)
                if (_activeProcesses.ContainsKey(task.Id)) 
                {

                    DaemonActiveProcess done = _activeProcesses[task.Id];
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

                    _activeProcesses.Remove(task.Id);
                }
        }

        public bool HasActive(string key)
        {
            lock (ProgramStart.LockInstance)
                return _activeProcesses.ContainsKey(key);
        }

        public bool HasActive(DaemonTask task)
        {
            lock (ProgramStart.LockInstance)
                return _activeProcesses.ContainsKey(task.Id);
        }

        public void ClearActive(DaemonTask task) 
        {
            lock (ProgramStart.LockInstance)
                if (_activeProcesses.ContainsKey(task.Id))
                    _activeProcesses.Remove(task.Id);
        }

        public IEnumerable<DaemonDoneProcess> GetDone() 
        {
            lock (ProgramStart.LockInstance)
                return _doneProcesses.Where(d => d != null).OrderByDescending(d => d.DoneUTc);
        }

        public void ClearActive(string key)
        {
            lock (ProgramStart.LockInstance)
                if (_activeProcesses.ContainsKey(key))
                    _activeProcesses.Remove(key);
        }

        public IEnumerable<DaemonActiveProcess> GetAllActive() 
        {
            lock (ProgramStart.LockInstance)
                return _activeProcesses.Values;
        }

        public int GetAllActiveCount()
        {
            lock (ProgramStart.LockInstance)
                return _activeProcesses.Count;
        }

        public int GetAllActiveForTypeCount(Type type)
        {
            lock (ProgramStart.LockInstance)
                return _activeProcesses.Where(p => p.Value.Daemon == type).Count();
        }

        public IEnumerable<DaemonBlockedProcess> GetAllBlocked()
        {
            lock (ProgramStart.LockInstance)
                return _blockedProcesses.Values;
        }

        #endregion
    }
}
