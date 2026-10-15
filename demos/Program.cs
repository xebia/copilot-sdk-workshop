using GitHub.Copilot;
var demo = args.FirstOrDefault()?.ToLowerInvariant() ?? "help";
if (demo == "help")
{
    Console.WriteLine("""
        dotnet run --project demos -- <demo> [argument]

        check                 Verify runtime startup and authentication without a model call
        hello                 One message and response
        sessions              Conversation state across two messages
        streaming             Deltas, tool activity and session lifecycle
        tools                 Build-status tool from the slides
        investigate           Multiple tools investigate a blocked deployment
        mcp <server-dll>       Connect to the local workshop MCP server
        persist <session-id>  Create a durable conversation
        resume <session-id>   Resume that conversation in a later process
        permissions           Demonstrate a denied tool request
        multi-agent           Two independent specialists and a coordinator
        challenge             Release-readiness investigation

        Optional environment variable: COPILOT_MODEL (default: auto)
        """);
    return;
}

var knownDemos = new[] { "check", "hello", "sessions", "streaming", "tools", "investigate", "mcp",
    "persist", "resume", "permissions", "multi-agent", "challenge" };
if (!knownDemos.Contains(demo))
    throw new ArgumentException($"Unknown demo '{demo}'. Run with 'help' for usage.");
if (demo is "persist" or "resume" or "mcp" && args.Length != 2)
    throw new ArgumentException($"The {demo} demo requires exactly one argument. Run with 'help' for usage.");
if (demo is not ("persist" or "resume" or "mcp") && args.Length != 1)
    throw new ArgumentException($"The {demo} demo does not accept additional arguments.");

var demos = new Dictionary<string, IDemo>
{
    ["check"] = new CheckDemo(),
    ["hello"] = new HelloDemo(),
    ["sessions"] = new SessionsDemo(),
    ["streaming"] = new StreamingDemo(),
    ["tools"] = new ToolsDemo(),
    ["investigate"] = new InvestigateDemo(),
    ["mcp"] = new McpDemo(),
    ["persist"] = new PersistDemo(),
    ["resume"] = new ResumeDemo(),
    ["permissions"] = new PermissionsDemo(),
    ["multi-agent"] = new MultiAgentDemo(),
    ["challenge"] = new ChallengeDemo()
};

await using var client = new GitHub.Copilot.CopilotClient();
await demos[demo].RunAsync(client, args);
