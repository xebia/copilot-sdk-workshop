using GitHub.Copilot;
using GitHub.Copilot.Rpc;
using Workshop.Tools;

internal sealed class PersistDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        var config = WorkshopSession.Configure([]);
        config.SessionId = args[1];
        await using var session = await client.CreateSessionAsync(config);
        Console.WriteLine(await WorkshopSession.Ask(session,
            "Remember: our release candidate is checkout v2.4.1. We have not approved release yet."));
        Console.WriteLine($"Session '{args[1]}' is saved locally. Run the resume demo with the same ID.");
    }
}

internal sealed class ResumeDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        await using var session = await client.ResumeSessionAsync(args[1], new ResumeSessionConfig
        {
            AvailableTools = [],
            OnPermissionRequest = PermissionHandler.ApproveAll
        });
        Console.WriteLine(await WorkshopSession.Ask(session,
            "Which release candidate were we discussing, and did we approve it?"));
    }
}