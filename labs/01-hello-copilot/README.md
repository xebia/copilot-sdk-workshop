# Lab 1: Hello Copilot

**Time:** 15 minutes. **Deck sections:** Lab 01 and Four things just happened.

## Goal

Create the smallest useful application-to-Copilot path: client, session, message, response.

## Work in the starter

```bash
dotnet run --project labs/01-hello-copilot/start
```

Open `start/Program.cs`. The package reference is already installed.

1. Create a `CopilotClient` with `await using`.
2. Create a session with `CreateSessionAsync`. Set `Model = "auto"`, `AvailableTools = []`, and `OnPermissionRequest = PermissionHandler.ApproveAll`.
3. Send this prompt with `SendAndWaitAsync`: `Explain dependency injection in one sentence.`
4. Print `response.Data.Content`. If the response has no content, throw an exception rather than printing a blank line.
5. Use a timeout such as `TimeSpan.FromMinutes(3)`.

The empty tool allowlist is intentional: this exercise needs conversation, not filesystem or shell access.

## Acceptance checklist

- The project builds and prints an assistant answer.
- The answer addresses dependency injection.
- Client and session are disposed asynchronously.
- You can identify which line creates the client, the session, the message and the wait for idle.

`SendAndWaitAsync` waits for the agent turn to become idle, not just a single model completion.

## Bonus: conversation state

Using the **same session**, first send `My name is Fred.` and then `What is my name?`. Do not manually replay the first message. Compare with a fresh session, which has not been told the name.

## Solution

```bash
dotnet run --project labs/01-hello-copilot/solution
dotnet run --project demos -- sessions
```

Authentication problems are setup problems, not a reason to change this into a direct HTTP model call. See the root README's troubleshooting table.
