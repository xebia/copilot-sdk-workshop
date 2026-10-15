using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Workshop.Tests;

public sealed class McpTests
{
    [Fact]
    public async Task Stdio_server_discovers_and_executes_tools_without_Copilot_or_credentials()
    {
        var server = typeof(Workshop.McpServer.ServiceTools).Assembly.Location;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var client = await McpClient.CreateAsync(
            new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = "dotnet",
                Arguments = [server],
                Name = "workshop-test"
            }),
            cancellationToken: timeout.Token);
        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        Assert.Equal(new[] { "get_incidents", "get_service_status" }, tools.Select(t => t.Name).Order());

        var result = await client.CallToolAsync("get_service_status",
            new Dictionary<string, object?> { ["service"] = "checkout" },
            cancellationToken: timeout.Token);
        Assert.False(result.IsError is true);
        Assert.Contains("degraded", string.Join("", result.Content.OfType<TextContentBlock>().Select(c => c.Text)));

        var incidents = await client.CallToolAsync("get_incidents",
            new Dictionary<string, object?> { ["service"] = "checkout" },
            cancellationToken: timeout.Token);
        Assert.Contains("INC-104", string.Join("", incidents.Content.OfType<TextContentBlock>().Select(c => c.Text)));

        var invalid = await client.CallToolAsync("get_service_status",
            new Dictionary<string, object?> { ["service"] = "missing" },
            cancellationToken: timeout.Token);
        Assert.True(invalid.IsError);
    }
}
