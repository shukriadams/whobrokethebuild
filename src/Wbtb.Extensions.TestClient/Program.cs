using System;
using System.Threading.Tasks;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Work(args).Wait();
            // or, if you want to avoid exceptions being wrapped into AggregateException:
            //  MainAsync().GetAwaiter().GetResult();
        }

        static async Task Work(string[] args)
        {
            User user = new User();
            //IDataLayer datalayer = PluginProvider.GetSingle(typeof(IDataLayer)) as IDataLayer;
            // do stuff here
            //user = await datalayer.SaveUser(user);
            //Console.WriteLine("id : " + user.Id);
        }
    }
}
