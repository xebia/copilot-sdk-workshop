using GitHub.Copilot;
using Microsoft.Extensions.AI;

namespace Workshop.Tools;

public static class WorkshopSession
{
    public static SessionConfig Configure(AIFunction[] tools, bool streaming = false) => new()
    {
        Model = Environment.GetEnvironmentVariable("COPILOT_MODEL") ?? "auto",
        Tools = [.. tools],
        AvailableTools = tools.Select(tool => tool.Name).ToList(),
        Streaming = streaming,
        OnPermissionRequest = PermissionHandler.ApproveAll,
        SystemMessage = new SystemMessageConfig
        {
            Content = """
                You are a workshop investigator using fictional, read-only data.
                Use the available tools to obtain domain facts; do not invent facts.
                Explain which evidence supports your conclusion and what is still unknown.
                Do not claim that a degraded status proves a root cause.
                Be concise. Do not use em dashes.
                """
        }
    };

    public static IDisposable Observe(CopilotSession session, bool streaming = false) =>
        session.On<SessionEvent>(ev =>
        {
            switch (ev)
            {
                case ToolExecutionStartEvent started:
                    Console.Error.WriteLine($"[tool:start] {started.Data.ToolName} {started.Data.Arguments}");
                    break;
                case ToolExecutionCompleteEvent completed:
                    Console.Error.WriteLine($"[tool:complete] {completed.Data.ToolCallId} success={completed.Data.Success}");
                    break;
                case SessionErrorEvent error:
                    Console.Error.WriteLine($"[session:error] {error.Data.Message}");
                    break;
                case AssistantMessageDeltaEvent delta when streaming:
                    Console.Write(delta.Data.DeltaContent);
                    break;
                case SessionIdleEvent when streaming:
                    Console.WriteLine();
                    break;
            }
        });

    public static async Task<string> Ask(CopilotSession session, string prompt)
    {
        var response = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = prompt }, timeout: TimeSpan.FromMinutes(3));
        return !string.IsNullOrWhiteSpace(response?.Data.Content)
            ? response.Data.Content
            : throw new InvalidOperationException("The session became idle without an assistant answer.");
    }
}
