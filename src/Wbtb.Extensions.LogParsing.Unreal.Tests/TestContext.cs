using Moq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Unreal.Tests
{
    public class TestContext
    {
        public TestContext()
        {
            SimpleDI di = new  SimpleDI();
            Mock<IDataPlugin> data = new Mock<IDataPlugin>();
            Mock<IPluginProvider> plugins = new Mock<IPluginProvider>();
            plugins
                .Setup(r => r.GetFirstForInterface<IDataPlugin>(It.IsAny<bool>()))
                .Returns(data.Object);
            
            Configuration configuration = new Configuration();
            Logger logger = new Logger();

            di.Register<Unreal4, Unreal4>();
            di.Register<Cache, Cache>();
            di.RegisterSingleton<IDataPlugin>(data.Object);
            di.RegisterSingleton<Configuration>(configuration);
            di.RegisterSingleton<Logger>(logger);
            di.RegisterSingleton<IPluginProvider>(plugins.Object);
        }
    }    
}

