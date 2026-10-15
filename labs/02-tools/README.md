# Lab 2: Custom Tool

**Time:** 15 minutes. **Deck sections:** Capabilities, custom-tool demo and Lab 02.

## Goal

Expose `get_service_status(service)` and let the agent decide when to use it.

```bash
dotnet run --project labs/02-tools/start
```

## Build the capability

Open `start/Program.cs`. Reuse your Lab 1 session code or consult its solution.

1. Define a tool with `CopilotTool.DefineTool`.
2. Give the string `service` parameter a `[Description(...)]` attribute.
3. Use `AIFunctionFactoryOptions` to name the tool `get_service_status` and describe what it returns.
4. Return a structured object containing the requested service and its status.
5. Throw an `ArgumentException` with known service names for unknown input. Do not turn invalid input into a fake healthy result.
6. Register the tool in `SessionConfig.Tools` and add `get_service_status` to `AvailableTools`.
7. Subscribe to `ToolExecutionStartEvent` before sending the message. Log tool activity to stderr.
8. Ask: **Why might checkout currently be unavailable?**

| Service | Status |
|---|---|
| checkout | degraded |
| payments | healthy |
| catalog | healthy |

Do not implement `if (prompt.Contains(...))` logic. Expose a capability, not an application-side investigation workflow.

## Acceptance checklist

- The agent invokes `get_service_status` with `checkout`.
- The answer reports degraded health based on the tool result.
- The answer does **not** claim to know the root cause from a health status alone.
- Asking about payments returns healthy; asking about an unknown service does not fabricate data.

## Bonus: incidents

Add `get_incidents(service)` with this fictional evidence:

```text
checkout:
  INC-104
  investigating
  Checkout requests time out after a connection-pool limit change in v2.4.1.

payments: no incidents
catalog: no incidents
```

Register both tools and repeat the question. The agent should now be able to connect the degraded status to the reported incident while acknowledging that the investigation is ongoing.

## Solutions

```bash
# Minimum version: only the health tool is exposed
dotnet run --project labs/02-tools/solution

# Bonus: expose both health and incident tools
dotnet run --project labs/02-tools/solution -- --bonus
```

The solution keeps the complete tool definition in `solution/Program.cs` so you can inspect the delegate, metadata, registration and event subscription in one place.
