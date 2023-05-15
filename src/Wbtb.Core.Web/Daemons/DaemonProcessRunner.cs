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
