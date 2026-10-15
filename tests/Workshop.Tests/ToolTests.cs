using System.Text.Json;
using Microsoft.Extensions.AI;
using Workshop.Tools;
using Xunit;

namespace Workshop.Tests;

public sealed class ToolTests
{
    [Theory]
    [InlineData("devops", 4)]
    [InlineData("github", 3)]
    [InlineData("support", 3)]
    [InlineData("release", 4)]
    public void Tracks_have_unique_documented_tools_and_exact_allowlists(string track, int count)
    {
        var tools = ToolCatalog.ForTrack(track);
        Assert.Equal(count, tools.Length);
        Assert.Equal(count, tools.Select(t => t.Name).Distinct().Count());
        foreach (var tool in tools)
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Description));
            var properties = tool.JsonSchema.GetProperty("properties");
            Assert.NotEmpty(properties.EnumerateObject());
            foreach (var parameter in properties.EnumerateObject())
                Assert.False(string.IsNullOrWhiteSpace(parameter.Value.GetProperty("description").GetString()));
        }

        var config = WorkshopSession.Configure(tools);
        Assert.Equal(tools.Select(t => t.Name), config.AvailableTools!);
        Assert.Equal(tools.Select(t => t.Name), config.Tools!.Select(t => t.Name));
    }

    [Fact]
    public void Hello_configuration_exposes_no_tools() =>
        Assert.Empty(WorkshopSession.Configure([]).AvailableTools!);

    [Fact]
    public async Task Tools_bind_arguments_and_return_serializable_structured_evidence()
    {
        var cases = new (AIFunction Tool, string Parameter, string Value, string Expected)[]
        {
            (ToolCatalog.BuildStatus(), "project", "api", "failing"),
            (ToolCatalog.ServiceStatus(), "service", "checkout", "degraded"),
            (ToolCatalog.Incidents(), "service", "checkout", "INC-104"),
            (ToolCatalog.DeploymentStatus(), "service", "payments", "blocked"),
            (ToolCatalog.PullRequests(), "repository", "payments", "42"),
            (ToolCatalog.Repository(), "repository", "checkout", "Checkout"),
            (ToolCatalog.Issues(), "repository", "checkout", "critical"),
            (ToolCatalog.Customer(), "customerId", "123", "Alex Example"),
            (ToolCatalog.Orders(), "customerId", "123", "ORD-9001"),
            (ToolCatalog.Tests(), "project", "checkout", "ConnectionPoolRecoversAfterTimeout"),
            (ToolCatalog.SecurityFindings(), "project", "checkout", "SEC-17")
        };
        foreach (var (tool, parameter, value, expected) in cases)
        {
            var result = await tool.InvokeAsync(new AIFunctionArguments { [parameter] = value });
            Assert.Contains(expected, JsonSerializer.Serialize(result));
        }
    }

    [Fact]
    public void Unknown_tracks_fail_explicitly() =>
        Assert.Throws<ArgumentException>(() => ToolCatalog.ForTrack("missing"));
}
