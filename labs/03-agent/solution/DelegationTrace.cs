using GitHub.Copilot;

namespace Workshop.Lab03;

public sealed class DelegationTrace(TextWriter output)
{
    private readonly object gate = new();
    private readonly Dictionary<string, string> running = [];
    private readonly HashSet<string> completed = [];
    private readonly List<string> failures = [];

    public void Record(SessionEvent ev)
    {
        lock (gate)
        {
            switch (ev)
            {
                case SubagentStartedEvent started:
                    running[started.Data.ToolCallId] = started.Data.AgentName;
                    if (!Investigation.SpecialistTools.ContainsKey(started.Data.AgentName))
                        failures.Add($"Unexpected specialist: {started.Data.AgentName}");
                    output.WriteLine($"[subagent:start] {started.Data.AgentName} call={started.Data.ToolCallId}");
                    break;
                case SubagentCompletedEvent done:
                    if (!running.Remove(done.Data.ToolCallId, out var name) || name != done.Data.AgentName)
                        failures.Add($"Completion without a matching start: {done.Data.AgentName}");
                    else if (done.Data.Cancelled is true)
                        failures.Add($"Cancelled specialist: {name}");
                    else
                        completed.Add(name);
                    output.WriteLine($"[subagent:complete] {done.Data.AgentName} call={done.Data.ToolCallId} cancelled={done.Data.Cancelled is true}");
                    break;
                case SubagentFailedEvent failed:
                    running.Remove(failed.Data.ToolCallId);
                    failures.Add($"{failed.Data.AgentName}: {failed.Data.Error}");
                    output.WriteLine($"[subagent:failed] {failed.Data.AgentName} call={failed.Data.ToolCallId} {failed.Data.Error}");
                    break;
                case ToolExecutionStartEvent tool:
                    output.WriteLine($"[tool:start] {tool.Data.ToolName} call={tool.Data.ToolCallId} agent={tool.AgentId ?? "parent/unattributed"} {tool.Data.Arguments}");
                    break;
                case ToolExecutionCompleteEvent tool:
                    output.WriteLine($"[tool:complete] call={tool.Data.ToolCallId} agent={tool.AgentId ?? "parent/unattributed"} success={tool.Data.Success}");
                    break;
                case SessionErrorEvent error:
                    failures.Add(error.Data.Message);
                    output.WriteLine($"[session:error] {error.Data.Message}");
                    break;
            }
        }
    }

    public void EnsureComplete()
    {
        lock (gate)
        {
            var missing = Investigation.SpecialistTools.Keys.Except(completed).ToArray();
            if (failures.Count != 0 || running.Count != 0 || missing.Length != 0)
                throw new InvalidOperationException(
                    $"Delegation incomplete. Missing successful reports: {string.Join(", ", missing)}. " +
                    $"Still running: {string.Join(", ", running.Values)}. Failures: {string.Join("; ", failures)}");
        }
    }
}
