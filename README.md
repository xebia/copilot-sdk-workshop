# GitHub Copilot SDK workshop

Runnable C# examples and hands-on labs for [the presentation](copilot-sdk-workshop.html).

The progression is **session, custom tool, MCP and policy boundaries, then file-defined specialists and runtime delegation**. All service, customer, repository and release data is fictional and hardcoded. The only external service required for the agent exercises is GitHub Copilot itself.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). `global.json` accepts installed .NET 10 feature bands.
- Git and a C# editor.
- GitHub Copilot access, an authenticated account, and permission to use the selected model.
- Network access to NuGet, GitHub release downloads and Copilot endpoints. Model calls consume your account's Copilot allowance.

The pinned `GitHub.Copilot.SDK` package downloads its matching CLI runtime during build. You do not need a separate CLI installation to **run** these examples.

For authentication, reuse an existing Copilot sign-in. If you have not signed in, install [Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli/install-copilot-cli), run `copilot`, and follow the sign-in flow. Alternatively, supply a supported token through `COPILOT_GITHUB_TOKEN` using your shell or secret manager. Do not put tokens in source files, committed configuration or lab handouts. See the [SDK authentication guide](https://github.com/github/copilot-sdk/blob/main/docs/auth/authenticate.md) for supported token types and permissions.

## Start here

Run these commands from the repository root:

```bash
dotnet restore CopilotSdkWorkshop.slnx
dotnet build CopilotSdkWorkshop.slnx --no-restore
dotnet run --project demos -- check
dotnet run --project labs/01-hello-copilot/solution
```

`check` starts the bundled runtime and checks authentication without sending a model prompt. It fails explicitly if authentication is unavailable.

The examples use model `auto`. To choose another available model, set the optional `COPILOT_MODEL` environment variable. On macOS/Linux:

```bash
export COPILOT_MODEL=auto
```

In PowerShell:

```powershell
$env:COPILOT_MODEL = "auto"
```

## Labs

Each lab contains a buildable `start/` project with TODOs, a complete `solution/` project, and its own instructions. Starter projects intentionally print only the exercise name until you implement the TODOs.

| Exercise | Time | Instructions | Run the starter |
|---|---|---|---|
| Lab 1: Hello Copilot | 15 min | [Lab 1](labs/01-hello-copilot/README.md) | `dotnet run --project labs/01-hello-copilot/start` |
| Lab 2: Custom Tool | 15 min | [Lab 2](labs/02-tools/README.md) | `dotnet run --project labs/02-tools/start` |
| Lab 3: File-defined specialists | 20 min | [Lab 3](labs/03-agent/README.md) | `dotnet run --project labs/03-agent/start` |
| Optional release challenge | 10 min | [Challenge](labs/04-release-challenge/README.md) | `dotnet run --project labs/04-release-challenge/start` |

Solutions:

```bash
dotnet run --project labs/01-hello-copilot/solution
dotnet run --project labs/02-tools/solution
dotnet run --project labs/02-tools/solution -- --bonus
dotnet run --project labs/03-agent/solution
dotnet run --project labs/04-release-challenge/solution -- checkout
dotnet run --project labs/04-release-challenge/solution -- payments
```

Lab 3 follows the advanced concepts and replaces the old three-track tool exercise. A parent delegates one customer-incident investigation to customer and service specialists defined in `.agent.md` files. Participants can do every lab from the slides before receiving the starter or reference solution.

## Presenter demos

```bash
dotnet run --project demos -- help
```

[Demo guide](demos/README.md) maps commands to the slide deck, including streaming, custom tools, a real local MCP connection, persistence, permissions, and application-orchestrated specialist agents. That last demo intentionally contrasts with Lab 3's runtime-managed delegation.

## Repository layout

```text
copilot-sdk-workshop.html       Presentation
demos/                         Command-selectable SDK examples
  mcp-server/                  Local read-only stdio MCP server
labs/
  01-hello-copilot/             Client, session, message, response
  02-tools/                    Tool definition and bonus incident lookup
  03-agent/                    File-defined customer/service specialists
  04-release-challenge/         Optional final investigation
shared/Workshop.Tools/         Typed fake data, reusable tools and demo helpers
tests/Workshop.Tests/          Data, tool-binding and stdio MCP tests
```

## Safety and scope

Examples explicitly allowlist their own tools with `AvailableTools`. Lab 3 additionally allows the runtime's `task` dispatch tool, hides domain tools from the parent with `DefaultAgent.ExcludedTools`, and scopes each specialist in its agent file. The examples do not grant shell, filesystem editing or deployment capabilities. `ApproveAll` is used only for these scoped workshop capabilities; the permissions demo instead denies requests. This is not a production permission policy or a sandbox.

The SDK can still maintain session data on your machine. Do not use confidential prompts or real customer information in a classroom. Customer, order and service investigations use fake local data, not real customers or services.

The release challenge produces an advisory answer. It does not authorize or execute a release. A real application must enforce release policy with deterministic checks and human approvals, not solely an LLM's recommendation.

Each model wait has a three-minute timeout. A timeout stops the wait, not necessarily the underlying agent work. These short-lived programs dispose their session and client when they exit. If investigating timeouts in a long-running application, explicitly manage cancellation and `AbortAsync`.

## Validation

```bash
dotnet test tests/Workshop.Tests/Workshop.Tests.csproj
```

Tests send no model prompts. They cover fixture consistency, invalid input, actual SDK function binding and schemas, tool allowlists, and discovery/invocation/error handling over a real stdio MCP connection. Lab 3 tests also start the bundled runtime to inspect native agent discovery and effective tool scopes without model calls, and test missing/failed/cancelled report handling. Agent reasoning remains nondeterministic; use each lab's acceptance checklist during authorized live runs rather than expecting exact wording, delegation order or tool order.

## Troubleshooting

| Symptom | Check |
|---|---|
| SDK selection fails | Install .NET 10 and run `dotnet --version` from this folder. |
| Restore/build cannot download runtime | Check NuGet access, GitHub release access, proxy configuration and your organisation's certificate settings. Do not disable TLS verification. |
| Authentication fails | Run the `check` demo, verify your Copilot sign-in and account access, and check whether an expired environment token overrides your stored login. |
| Model unavailable or disallowed | Use `auto`, or set `COPILOT_MODEL` to a model available to your account and organisation. |
| Tool is never called | Check its registration, allowlist, description and parameter descriptions. Ask a question that needs the data. |
| Lab 2 invents a root cause | The minimum tool only knows health. Ask the agent to state uncertainty, then implement the incident tool. |
| Lab 3 cannot discover specialists | Build and check both output `agents/*.agent.md` files. Native discovery uses the application output path, not your shell's directory. |
| Lab 3 answers without delegation | Check `task`, parent-only exclusions, role descriptions and both specialists' lifecycle events. A final answer is not enough. |
| MCP demo cannot start | Build the MCP server first and pass the compiled DLL path, not a project folder. |
| Resume fails | Use the same machine, OS user, runtime storage and session ID as the persist demo. |

Versions are pinned centrally in `Directory.Packages.props`. Updating a package should be followed by rebuilding, rerunning the tests, and rehearsing authenticated demos.
