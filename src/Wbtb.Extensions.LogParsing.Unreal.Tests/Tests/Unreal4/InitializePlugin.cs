using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.LogParsing.Unreal.Tests
{
    public class LogTest
    {
        private TestContext testContext = new TestContext();
        
        [Fact]
        public void Happy_Path()
        {
            SimpleDI di = new SimpleDI();
            ILogParserPlugin unreal4 = di.Resolve<Unreal4>();
            PluginInitResult result = unreal4.InitializePlugin();
            Assert.True(result.Success);
        }
    }
}
