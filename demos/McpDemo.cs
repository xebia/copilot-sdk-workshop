using GitHub.Copilot;
using GitHub.Copilot.Rpc;
using Workshop.Tools;

internal sealed class McpDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        var server = Path.GetFullPath(args[1]);
        if (!File.Exists(server)) throw new FileNotFoundException("Build the workshop MCP server first.", server);
        var config = WorkshopSession.Configure([]);
        config.McpServers = new Dictionary<string, McpServerConfig>
        {
            ["workshop"] = new McpStdioServerConfig
            {
                Command = "dotnet",
                Args = [server],
                Tools = ["get_service_status", "get_incidents"],
                Timeout = 30_000
            }
        };
        config.AvailableTools = ["mcp:workshop-get_service_status", "mcp:workshop-get_incidents"];
        await using var session = await client.CreateSessionAsync(config);
        using var events = WorkshopSession.Observe(session);
        Console.WriteLine(await WorkshopSession.Ask(session, "Why might checkout currently be unavailable?"));
    }
}