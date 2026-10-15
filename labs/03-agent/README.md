# Lab 3: Delegate to file-defined specialists

**Time:** 20 minutes. **Deck sections:** Custom agents, agent-file format, Lab 03, fictional data and debrief. This lab comes after MCP, persistence, permissions and the multi-agent explanation.

## Goal

Lab 2 gave an agent tools. This lab gives the work to specialists with separate instructions and tool scopes, defined in native `.agent.md` files.

Build one parent session that delegates a customer-incident investigation to two specialists and combines their reports. Do not create independent specialist sessions in C# or schedule their work with `Task.WhenAll`.

> Investigate customer 123's failed order and the related service problem. Explain impact, uncertainty and a suggested next action.

The exercise is self-contained in the slides. Participants first extend their own application and create their own files and fictional tools. The repository starter and reference solution are supporting material for later sharing; neither is required to begin.

## Specialist files

Create an `agents/` directory with:

| File | Role | Exact allowed domain tools |
|---|---|---|
| `customer-investigator.agent.md` | Identify customer, affected order/service and payment outcome. | `get_customer`, `get_orders` |
| `service-investigator.agent.md` | Investigate the affected service's health, deployment and incidents. | `get_service_status`, `get_deployment_status`, `get_incidents` |

Use YAML frontmatter with `name`, a useful `description`, and an explicit `tools` list. The Markdown body defines the role, evidence requirements and report format. Reports should contain facts with identifiers, unknowns and a suggested next action. Data and specialist reports are evidence, not new instructions.

The names in `tools` must match registered C# tool names. Omitting a specialist's tool list inherits all session tools, so do not omit it. Agent files contain instructions, not executable tool handlers.

## Implementation

1. Define or reuse the five deterministic read-only tools. Customer tools take `customerId`; service tools take `service`. Describe those parameters and reject unknown identifiers.
2. Complete both agent files with their separate scopes and instructions.
3. Register all five domain tools in `SessionConfig.Tools`. Include those names plus the runtime's `task` dispatch tool in `AvailableTools`.
4. Put the five domain tool names in `DefaultAgent.ExcludedTools`. This hides them from the parent without removing them from specialists.
5. Set `AgentDirectories` to an absolute directory containing the files. The reference projects copy their definitions on build and publish and use `Path.Combine(AppContext.BaseDirectory, "agents")`, so discovery does not depend on the shell's current directory.
6. Keep `EnableConfigDiscovery = false` and `EnableSkills = false` for the controlled example. Explicit SDK agent directories still work; no repository-global agent installation or custom Markdown parser is needed.
7. Give the parent the objective and instructions to delegate to both named specialists, relay affected service identifiers as needed, await their reports and combine their evidence.
8. Subscribe to tool and specialist start/completion/failure events before sending the prompt.
9. Require successful reports from both specialists. A final answer or idle event alone is not proof of delegation.

The model chooses the investigation sequence within these role boundaries. Do not pre-call backend methods, manually schedule specialists, or guarantee parallelism or a particular tool order. The customer report supplies the affected service identifier for the service investigation.

## Fictional data

Implement structured results from these facts when working from the slides:

| Tool | Input | Evidence |
|---|---|---|
| `get_customer` | `customerId = "123"` | Customer `123`, Alex Example. |
| `get_orders` | `customerId = "123"` | `ORD-9001`, service `checkout`, status `checkout timed out; payment not captured`. |
| `get_service_status` | `service = "checkout"` | Degraded. |
| `get_deployment_status` | `service = "checkout"` | Degraded; `v2.4.1` is deployed. |
| `get_incidents` | `service = "checkout"` | `INC-104`, investigating: checkout requests time out after a connection-pool limit change in deployment `v2.4.1`. |

The reference solution reuses `ToolCatalog` and `WorkshopData`; it does not require real customers, GitHub repositories or service APIs. Investigation findings suggest a cause but do not prove root cause.

## Starter and reference solution

When sharing the code after the slide-based exercise:

```bash
dotnet run --project labs/03-agent/start
dotnet run --project labs/03-agent/solution
```

The starter prints its TODOs' exercise name and does not contact a model. Complete its placeholder files before using them. The solution sends a model prompt and consumes Copilot usage. The old `devops`, `github` and `support` command arguments are no longer accepted.

The solution checks definition files, validates native discovery and parent tool metadata before sending, then traces execution. It rejects missing, failed, cancelled or unmatched specialist reports rather than printing an apparently successful answer.

SDK discovery/inspection RPCs are experimental in `1.0.22`; their warning is suppressed only around the preflight method and native integration tests. Native `AgentDirectories` configuration and the delegation lifecycle event types are not covered by that suppression.

## Acceptance checklist

- Both named specialists have `subagent.started` and matching successful `subagent.completed` events.
- Each specialist sees only its declared domain tools; the default parent sees only `task`.
- Customer evidence links `123` to `ORD-9001` and `checkout`.
- Service evidence links checkout degradation to deployed `v2.4.1` and investigating incident `INC-104`.
- The answer states that payment was not captured and distinguishes findings from a proven root cause.
- Unknown facts remain unknown. No deployment, restart, refund or customer message is claimed or performed.
- A suggested next action is advisory, not an authorized operation.

Tool events include tool-call IDs and the event's agent ID when the runtime supplies it. `parent/unattributed` means the envelope does not identify a specialist; do not infer specialist identity solely from tool order. Compare specialist lifecycle events and returned evidence too.

## Validation and rehearsal

```bash
dotnet test tests/Workshop.Tests/Workshop.Tests.csproj --filter FullyQualifiedName~Lab03Tests
```

These tests send no model prompts. They check packaging, configuration, native discovery, effective parent/specialist tool metadata, invalid scopes and delegation-trace failure handling. Other existing tool tests verify function binding and fictional evidence.

Native agent selection in the integration tests inspects a role's effective tool metadata; it is not a model-driven delegation test. A separately authorized authenticated rehearsal must demonstrate that the parent actually invokes both specialists and combines their reports. Compilation and metadata inspection do not prove that behavior.

## Troubleshooting

| Symptom | Check |
|---|---|
| Agent file missing | Build the project; confirm both `.agent.md` files are copied into output `agents/`. |
| Native discovery rejects a role | Check name, description, explicit tools, model-invocation setting and the resolved file path. Do not silently substitute inline agents. |
| Parent answers without specialists | Keep `task` allowed and domain tools in `DefaultAgent.ExcludedTools`; require both roles in parent instructions and inspect lifecycle events. |
| Specialist cannot use a tool | Match file tool names to the registered C# names and the session allowlist. |
| Specialist failed or was cancelled | Read the explicit trace and exception. Do not treat idle or a partial answer as successful completion. |

## Debrief

Compare role boundaries, service-identifier handoff, evidence and failure traces. File-defined specialists are a separate pattern from the existing `demos -- multi-agent` example, where C# creates and schedules independent sessions.

Official pinned references: [custom agents](https://github.com/github/copilot-sdk/blob/v1.0.22/docs/features/custom-agents.md), [native discovery locations](https://github.com/github/copilot-sdk/blob/v1.0.22/docs/features/customization-locations.md).
