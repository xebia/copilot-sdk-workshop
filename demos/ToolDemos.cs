using GitHub.Copilot;
using Microsoft.Extensions.AI;
using Workshop.Tools;

internal abstract class ToolDemo : IDemo
{
    protected abstract AIFunction[] Tools { get; }
    protected abstract string Prompt { get; }
    protected virtual bool Streaming => false;

    public async Task RunAsync(CopilotClient client, string[] args)
    {
        await using var session = await client.CreateSessionAsync(WorkshopSession.Configure(Tools, streaming: Streaming));
        using var events = WorkshopSession.Observe(session, streaming: Streaming);
        var answer = await WorkshopSession.Ask(session, Prompt);
        if (!Streaming) Console.WriteLine(answer);
    }
}

internal sealed class StreamingDemo : ToolDemo
{
    protected override AIFunction[] Tools => [ToolCatalog.ServiceStatus(), ToolCatalog.Incidents()];
    protected override string Prompt => "Why might checkout currently be unavailable?";
    protected override bool Streaming => true;
}

internal sealed class ToolsDemo : ToolDemo
{
    protected override AIFunction[] Tools => [ToolCatalog.BuildStatus()];
    protected override string Prompt => "Which of frontend, api and database currently have failing builds?";
}

internal sealed class InvestigateDemo : ToolDemo
{
    protected override AIFunction[] Tools => [ToolCatalog.DeploymentStatus(), ToolCatalog.BuildStatus(), ToolCatalog.PullRequests()];
    protected override string Prompt => "Why can't we deploy the payments service?";
}

internal sealed class ChallengeDemo : ToolDemo
{
    protected override AIFunction[] Tools => ToolCatalog.ForTrack("release");
    protected override string Prompt => "Is checkout ready to release? Examine build, tests, open pull requests and security evidence. Explain your decision.";
}