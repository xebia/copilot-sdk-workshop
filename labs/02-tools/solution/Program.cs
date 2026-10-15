using System.ComponentModel;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

if (args.Any(arg => arg != "--bonus"))
    throw new ArgumentException("The only supported argument is --bonus.");
var bonus = args.Contains("--bonus");

var getServiceStatus = CopilotTool.DefineTool(
    ([Description("Service name: checkout, payments, or catalog.")] string service) =>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        var status = service.Trim().ToLowerInvariant() switch
        {
            "checkout" => "degraded",
            "payments" or "catalog" => "healthy",
            _ => throw new ArgumentException($"Unknown service '{service}'. Use checkout, payments, or catalog.")
        };
        return new { service, status };
    },
    factoryOptions: new AIFunctionFactoryOptions
    {
        Name = "get_service_status",
        Description = "Gets a service's current health. A degraded status alone does not establish a root cause."
    });

var getIncidents = CopilotTool.DefineTool(
    ([Description("Service name: checkout, payments, or catalog.")] string service) =>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        return service.Trim().ToLowerInvariant() switch
        {
            "checkout" => new[]
            {
                new { id = "INC-104", service, status = "investigating",
                    summary = "Checkout requests time out after a connection-pool limit change in v2.4.1." }
            },
            "payments" or "catalog" => [],
            _ => throw new ArgumentException($"Unknown service '{service}'. Use checkout, payments, or catalog.")
        };
    },
    factoryOptions: new AIFunctionFactoryOptions
    {
        Name = "get_incidents",
        Description = "Lists current incidents and investigation findings for a service."
    });

await using var client = new CopilotClient();
await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = Environment.GetEnvironmentVariable("COPILOT_MODEL") ?? "auto",
    Tools = bonus ? [getServiceStatus, getIncidents] : [getServiceStatus],
    AvailableTools = bonus ? ["get_service_status", "get_incidents"] : ["get_service_status"],
    OnPermissionRequest = PermissionHandler.ApproveAll,
    SystemMessage = new SystemMessageConfig
    {
        Content = "Use tools for service facts. Explain uncertainty. Do not invent root causes. Be concise. Do not use em dashes."
    }
});

using var events = session.On<SessionEvent>(ev =>
{
    switch (ev)
    {
        case ToolExecutionStartEvent tool:
            Console.Error.WriteLine($"[tool] {tool.Data.ToolName} {tool.Data.Arguments}");
            break;
        case SessionErrorEvent error:
            Console.Error.WriteLine($"[error] {error.Data.Message}");
            break;
    }
});

var response = await session.SendAndWaitAsync(
    new MessageOptions { Prompt = "Why might checkout currently be unavailable?" },
    timeout: TimeSpan.FromMinutes(3));
if (string.IsNullOrWhiteSpace(response?.Data.Content))
    throw new InvalidOperationException("The session returned no assistant answer.");
Console.WriteLine(response.Data.Content);
