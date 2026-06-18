using System.Reflection;
using DataStatisticsService.Application.Messaging;
using Wolverine.Runtime.Handlers;

namespace DataStatisticsService.Application.Tests.Messaging;

public sealed class IngestedDataMessageHandlerPolicyTests
{
    [Fact]
    public void Handler_exposes_static_configure_for_wolverine_discovery()
    {
        var configure = typeof(IngestedDataMessageHandler).GetMethod(
            "Configure",
            BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(configure);
        Assert.Equal(typeof(HandlerChain), configure!.GetParameters()[0].ParameterType);
    }
}
