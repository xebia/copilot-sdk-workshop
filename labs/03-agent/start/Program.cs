using GitHub.Copilot;
using Workshop.Tools;

if (args.Length != 0) throw new ArgumentException("Lab 3 uses one customer-incident scenario. No arguments are required.");
Console.WriteLine("Lab 03: Delegate to file-defined specialists");

// TODO: Complete agents/customer-investigator.agent.md and agents/service-investigator.agent.md.
// TODO: Register Customer, Orders, ServiceStatus, DeploymentStatus and Incidents from ToolCatalog.
// TODO: Discover the files with AgentDirectories = [Path.Combine(AppContext.BaseDirectory, "agents")].
// TODO: Allow the five domain tools plus task. Exclude domain tools only from DefaultAgent.
// TODO: Give the parent instructions to delegate to both specialists and combine their evidence.
// TODO: Observe tool and subagent start/completion/failure events before sending the objective.
// TODO: Investigate customer 123's failed order, related service problem and suggested next action.
// TODO: Require both specialist reports; distinguish evidence, hypotheses and missing information.
// Use one parent session. Do not schedule specialist sessions or pre-call backend methods in C#.
