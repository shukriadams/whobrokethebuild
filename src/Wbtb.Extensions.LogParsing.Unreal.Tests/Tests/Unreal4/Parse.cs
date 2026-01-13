using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.LogParsing.Unreal.Tests.Tests;

public class Parse
{
    private TestContext testContext = new TestContext();
        
    /// <summary>
    /// Parse tries to find a Job from Build, should gracefully return error message in event of Job lookup to
    /// data backend returning null.
    /// </summary>
    [Fact]
    public void Job_Not_Found_Graceful_Handle()
    {
        SimpleDI di  = new SimpleDI();
        ILogParserPlugin unreal4 = di.Resolve<Unreal4>();
        string result = unreal4.Parse(new Build { JobId = "___"}, string.Empty);
        Assert.Equal($"Job ___ not found", result);
    }
}