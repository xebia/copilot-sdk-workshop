using GitHub.Copilot;
using GitHub.Copilot.Rpc;
using Workshop.Tools;

internal sealed class PermissionsDemo : IDemo
{
    public async Task RunAsync(CopilotClient client, string[] args)
    {
        var config = WorkshopSession.Configure([ToolCatalog.ServiceStatus()]);
        config.OnPermissionRequest = (request, _) =>
        {
            Console.Error.WriteLine($"[policy] Denied request kind: {request.Kind}");
#pragma warning disable GHCP001
            return Task.FromResult<PermissionDecision>(
                PermissionDecision.Reject("This demonstration denies all permission requests. Report the limitation; do not guess."));
#pragma warning restore GHCP001
        };
        await using var session = await client.CreateSessionAsync(config);
        using var events = WorkshopSession.Observe(session);
        Console.WriteLine(await WorkshopSession.Ask(session,
            "Use get_service_status to check checkout. If execution is denied, explain that you could not check it."));
    }
}