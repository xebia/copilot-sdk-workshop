# Presenter demo guide

Run commands from the repository root. Start with:

```bash
dotnet build CopilotSdkWorkshop.slnx
dotnet run --project demos -- check
```

All commands below except `check` send model prompts and consume Copilot usage. `help`, builds and the automated tests do not send prompts.

## Deck mapping

| Deck section | Concept | Command |
|---|---|---|
| Under the hood / Lab 01 | Client, session, message, response | `dotnet run --project demos -- hello` |
| Sessions | Session state | `dotnet run --project demos -- sessions` |
| Agent loop / Streaming | Observable loop and streaming deltas | `dotnet run --project demos -- streaming` |
| Capabilities / Custom-tool demo | Custom build-status tool | `dotnet run --project demos -- tools` |
| Multiple capabilities | Multiple tools investigate a deployment | `dotnet run --project demos -- investigate` |
| Custom tools versus MCP | Real tools across an MCP protocol boundary | See MCP section below |
| Persistence | Persistence across processes | See persistence section below |
| Permissions | Application-controlled permission denial | `dotnet run --project demos -- permissions` |
| Multiple agents / Start simple | Application-orchestrated specialist sessions | `dotnet run --project demos -- multi-agent` |
| Custom agent files / Lab 03 debrief | Runtime-managed file-defined specialists | `dotnet run --project labs/03-agent/solution` |
| Final challenge | Release-readiness challenge | `dotnet run --project demos -- challenge` |

## Source walkthrough

| Source | What to show |
|---|---|
| `demos/Program.cs` and `demos/*Demo.cs` | Command routing plus each demo's session creation, prompt and lifecycle |
| `shared/Workshop.Tools/ToolCatalog.cs` | `CopilotTool.DefineTool`, parameter metadata and tool descriptions |
| `shared/Workshop.Tools/WorkshopData.cs` | Small deterministic functions returning typed records |
| `shared/Workshop.Tools/WorkshopSession.cs` | Explicit allowlists, streaming configuration, event handling and idle wait |
| `labs/02-tools/solution/Program.cs` | A self-contained custom-tool example without the shared helper |
| `labs/03-agent/solution/Program.cs` | One parent session, native discovery preflight and delegation tracing |
| `labs/03-agent/solution/agents/` | File-defined customer and service roles with separate tool scopes |
| `demos/mcp-server/` | Actual stdio MCP host and read-only tools |

The tiny lab solutions are deliberately straightforward. The presenter demos share repetitive session and fixture code so they remain consistent.

## Streaming

The streaming demo sets `Streaming = true` and subscribes **before** sending the prompt. Assistant deltas go to stdout; tool and error diagnostics go to stderr. It still awaits `SendAndWaitAsync` to observe completion and errors, but does not print the final message a second time.

It exposes the health and incident tools, so one user message can produce multiple tool calls and multiple assistant messages.

The slide's event list is conceptual. Actual typed SDK events include `AssistantMessageDeltaEvent`, `ToolExecutionStartEvent`, `ToolExecutionCompleteEvent`, `SessionIdleEvent` and `SessionErrorEvent`.

## MCP: run a real local server

Build once, then pass the compiled DLL path:

```bash
dotnet build demos/mcp-server/Workshop.McpServer.csproj
dotnet run --project demos -- mcp demos/mcp-server/bin/Debug/net10.0/Workshop.McpServer.dll
```

The Copilot runtime launches this server as a child process using `dotnet <dll>`. There is no port to manage or service to leave running. The server uses the official C# MCP SDK and exposes only `get_service_status` and `get_incidents`.

The demo uses `McpStdioServerConfig` and a source-qualified allowlist:

```csharp
config.McpServers = new Dictionary<string, McpServerConfig>
{
    ["workshop"] = new McpStdioServerConfig
    {
        Command = "dotnet",
        Args = [server],
        Tools = ["get_service_status", "get_incidents"],
        Timeout = 30_000
    }
};
config.AvailableTools =
[
    "mcp:workshop-get_service_status",
    "mcp:workshop-get_incidents"
];
```

Stdout is reserved for MCP JSON-RPC; server logs go to stderr. The automated MCP test checks discovery, real invocation and an invalid-service error without calling Copilot.

This is a local stdio example, not an HTTP or GitHub MCP integration. It demonstrates the same protocol boundary without an additional access token.

## Persistence: two separate processes

Use a fresh, unique ID for a rehearsal:

```bash
dotnet run --project demos -- persist release-check-rehearsal-01
dotnet run --project demos -- resume release-check-rehearsal-01
```

The first process records checkout v2.4.1 and that release is not approved. The second should recall both facts without receiving them again.

Disposing a session releases live resources but does not delete its stored conversation. Resume uses the same local runtime storage and OS user. This is not remote storage or cross-machine synchronization. Custom delegate implementations, permission handlers and MCP configuration must be supplied again when a resumed application needs them; they are not executable code serialized into the transcript. This demonstration deliberately uses no tools on either side.

## Permissions

```bash
dotnet run --project demos -- permissions
```

Only `get_service_status` is available. The permission handler logs and rejects requests. A successful demonstration is a policy denial plus an answer that admits it could not obtain service health, not a fabricated status.

The permission decision API is marked experimental in the pinned SDK. The source suppresses that diagnostic only around the denial expression. Do not globally disable experimental or other compiler warnings.

Other examples use `ApproveAll` for explicit, read-only tool allowlists. Do not copy this into a broadly empowered production agent.

## Multiple agents

```bash
dotnet run --project demos -- multi-agent
```

The application creates two independent sessions:

1. Build specialist: build status and tests.
2. Review specialist: pull requests and security findings.

`Task.WhenAll` runs them concurrently. A third session receives only their reports and makes a recommendation. This is **application-orchestrated multi-agent work**, not an example of Fleet Mode or a model dynamically creating sub-agents. The separation is explicit so participants can see context isolation and the orchestration boundary.

Specialist reports are generated text, not guaranteed truth. A production coordinator needs evidence validation and deterministic policy enforcement. More agents also increase usage and failure modes.

After this contrast and the start-simple warning, introduce native `.agent.md` discovery and let participants do Lab 3 from the slides. Only show its reference implementation in the debrief. Unlike this demo, Lab 3 creates one parent session and lets the runtime delegate to its file-defined specialists. It does not use `Task.WhenAll` or manually create specialist sessions.

## Rehearsal checklist

- Restore and build before participants arrive, including the runtime download.
- Run all authenticated scenarios with the account/model you will use on stage.
- Verify that organisation policy allows the necessary tool use.
- Keep a solution window ready for each lab.
- Start session persistence with a fresh ID.
- Compare evidence and conclusions, not exact wording or call order.
- Keep MCP, persistence, permission and multi-agent sections short if time is tight.
- Treat the final release challenge as optional if the three labs use the available hands-on time.

Follow the deck's learning sequence: connection and Lab 1, tools and Lab 2, break and tool design, extension/policy concepts, then custom agent files and Lab 3. Save the optional release challenge and extra demos for follow-up if needed.
