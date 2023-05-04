using Grapevine.Server;

namespace MessageQueue
{
    public class HttpServer
    {
        private RestServer _server;

        public bool Verbose { get; set; }

        public void Start()
        {
            ServerSettings settings = new ServerSettings();
            settings.Host = "localhost";
            settings.Port = "5001";
            settings.Logger = null;

            _server = new RestServer(settings);


            if (this.Verbose)
                _server.LogToConsole();

            _server.Start();
        }

        public void Stop()
        {
            if (_server == null)
                return;

            _server.Stop();
            _server = null;
        }

    }
}