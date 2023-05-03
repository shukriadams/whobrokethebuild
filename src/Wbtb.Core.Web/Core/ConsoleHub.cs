using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Wbtb.Core.Web
{
    public class Test : IHubContext
    {
        public IHubClients Clients => throw new System.NotImplementedException();

        public IGroupManager Groups => throw new System.NotImplementedException();
    }
    public class ConsoleHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task MessageAll()
        {
            await Clients.All.SendAsync("ReceiveMessage", "some user", "some message");
        }
    }
}
