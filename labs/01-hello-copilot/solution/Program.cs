using GitHub.Copilot;

await using var client = new CopilotClient();
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = Environment.GetEnvironmentVariable("COPILOT_MODEL") ?? "auto",
    AvailableTools = [],
    OnPermissionRequest = PermissionHandler.ApproveAll
});

var response = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Explain dependency injection in one sentence." },
    timeout: TimeSpan.FromMinutes(3));

if (string.IsNullOrWhiteSpace(response?.Data.Content))
    throw new InvalidOperationException("The session returned no assistant answer.");

Console.WriteLine(response.Data.Content);
