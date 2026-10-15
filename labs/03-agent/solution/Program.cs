using GitHub.Copilot;
using Workshop.Lab03;
using Workshop.Tools;

if (args.Length != 0)
    throw new ArgumentException("Lab 3 now uses one customer-incident scenario. No track argument is required.");

var config = Investigation.CreateConfiguration(Path.Combine(AppContext.BaseDirectory, "agents"));
await using var client = new CopilotClient();
await using var session = await client.CreateSessionAsync(config);
using var startup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await Investigation.ValidateDiscoveryAsync(session, config.AgentDirectories![0], startup.Token);

var trace = new DelegationTrace(Console.Error);
using var events = session.On<SessionEvent>(trace.Record);
var answer = await WorkshopSession.Ask(session, Investigation.Prompt);
trace.EnsureComplete();
Console.WriteLine(answer);
