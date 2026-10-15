using GitHub.Copilot;
using Workshop.Tools;

if (args.Length > 1) throw new ArgumentException("Pass at most one project: checkout or payments.");
var project = args.FirstOrDefault()?.ToLowerInvariant() ?? "checkout";
if (project is not ("checkout" or "payments"))
    throw new ArgumentException("Choose checkout or payments.");

await using var client = new CopilotClient();
await using var session = await client.CreateSessionAsync(
    WorkshopSession.Configure(ToolCatalog.ForTrack("release")));
using var events = WorkshopSession.Observe(session);
Console.WriteLine(await WorkshopSession.Ask(session, $"""
    Is {project} ready to release?
    Check the available build, test, pull request and security evidence.
    Explain your recommendation, cite the concrete blockers and identify missing evidence.
    Do not execute a deployment. This is advisory, not an authorization to release.
    """));
