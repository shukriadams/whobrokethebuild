using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wbtb.Core.Web
{
    public class DaemonProcessRunner : IDaemonProcessRunner
    {
        private bool _running;

        private bool _busy;

        public DaemonProcessRunner()
        {
            _running = true;
        }

        public void dfStart(DaemonWork work, int tickInterval)
        {
            Task.Run(() => {
                while (_running)
                {
                    try
                    {
                        if (_busy)
                            return;

                        _busy = true;

                        work();
                    }
                    catch (Exception ex)
                    {
                        // must trap all exception in 
                        Console.WriteLine($"Unhandled exception from {this.GetType().Name} : {ex}");
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(tickInterval);
                    }
                }
            });
        }

        public void Start(DaemonWork work, int tickInterval)
        {
            // do each work tick on its own thread
            new Thread(delegate ()
            {
                while (_running)
                {
                    if (_busy)
                        return;

                    _busy = true;

                    try
                    {
                        work();
                    }
                    finally
                    {
                        _busy = false;
                    }

                    Console.WriteLine($"{this.GetType()} ticked");
                    Thread.Sleep(tickInterval);
                }

            }).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _running = false;
        }
    }
}
