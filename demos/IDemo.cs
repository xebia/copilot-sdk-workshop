using GitHub.Copilot;

internal interface IDemo
{
    Task RunAsync(CopilotClient client, string[] args);
}