using GitHub.Copilot;
using Workshop.Tools;

internal sealed class CheckDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await client.StartAsync(timeout.Token);
        var auth = await client.GetAuthStatusAsync(timeout.Token);
        Console.WriteLine("Bundled runtime started successfully.");
        Console.WriteLine($"Authenticated: {auth.IsAuthenticated}");
        if (!auth.IsAuthenticated)
            throw new InvalidOperationException("Sign in to Copilot or configure COPILOT_GITHUB_TOKEN before running the agent demos.");
    }
}

internal sealed class HelloDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        await using var session = await client.CreateSessionAsync(WorkshopSession.Configure([]));
        Console.WriteLine(await WorkshopSession.Ask(session, "Explain dependency injection in one sentence."));
    }
}

internal sealed class SessionsDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        await using var session = await client.CreateSessionAsync(WorkshopSession.Configure([]));
        Console.WriteLine(await WorkshopSession.Ask(session, "My name is Fred. Remember that for this conversation."));
        Console.WriteLine(await WorkshopSession.Ask(session, "What is my name?"));
    }
}