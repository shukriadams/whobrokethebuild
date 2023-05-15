namespace Wbtb.Core.Web
{
    public interface IDaemonProcessRunner
    {
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
