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

        public void Start(DaemonWork work, int tickInterval)
        {
            // wrap start in a risk so they don't block calling thread
            Task.Run(() => {
                while (_running)
                {
                    try
                    {
                        if (_busy)
                            return;

                        _busy = true;

                        // do each work tick on its own thread
                        ManualResetEvent resetEvent = new ManualResetEvent(false);
                        new Thread(delegate ()
                        {
                            try
                            {
                                work();
                            }
                            finally
                            {
                                resetEvent.Set();
                            }
                        }).Start();

                        // Wait for threads to finish
                        resetEvent.WaitOne();
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(tickInterval);
                    }
                }
            });
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
