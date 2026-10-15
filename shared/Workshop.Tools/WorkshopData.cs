namespace Workshop.Tools;

public sealed record BuildStatus(string Project, string Status);
public sealed record ServiceStatus(string Service, string Status);
public sealed record Incident(string Id, string Service, string Summary, string Status);
public sealed record DeploymentStatus(string Service, string Status, string Reason);
public sealed record PullRequest(int Number, string Title, bool BlocksRelease);
public sealed record RepositoryInfo(string Name, string Description);
public sealed record Issue(int Number, string Title, string Priority);
public sealed record Customer(string Id, string Name);
public sealed record Order(string Id, string CustomerId, string Status, string Service);
public sealed record TestRun(int Passed, int Failed, string[] Failures);
public sealed record SecurityFinding(string Id, string Severity, string Summary);

public static class WorkshopData
{
    public static BuildStatus GetBuildStatus(string project) => new(project, Key(project) switch
    {
        "frontend" or "database" or "payments" or "checkout" => "passing",
        "api" => "failing",
        _ => throw Unknown("project", project, "frontend, api, database, payments, checkout")
    });

    public static ServiceStatus GetServiceStatus(string service) => new(service, Key(service) switch
    {
        "checkout" => "degraded",
        "payments" or "catalog" => "healthy",
        _ => throw Unknown("service", service, "checkout, payments, catalog")
    });

    public static Incident[] GetIncidents(string service) => Key(service) switch
    {
        "checkout" =>
        [
            new("INC-104", "checkout",
                "Checkout requests time out after a connection-pool limit change in deployment v2.4.1.",
                "investigating")
        ],
        "payments" or "catalog" => [],
        _ => throw Unknown("service", service, "checkout, payments, catalog")
    };

    public static DeploymentStatus GetDeploymentStatus(string service) => Key(service) switch
    {
        "payments" => new(service, "blocked", "Release requires approval of blocking pull request #42."),
        "checkout" => new(service, "degraded", "v2.4.1 is deployed; see checkout incidents."),
        "catalog" => new(service, "deployed", "v1.8.0 is healthy."),
        _ => throw Unknown("service", service, "checkout, payments, catalog")
    };

    public static PullRequest[] GetOpenPullRequests(string repository) => Key(repository) switch
    {
        "payments" => [new(42, "Approve database migration for payments release", true)],
        "checkout" =>
        [
            new(73, "Restore checkout connection-pool limit", true),
            new(74, "Refresh checkout documentation", false)
        ],
        "catalog" => [],
        _ => throw Unknown("repository", repository, "checkout, payments, catalog")
    };

    public static RepositoryInfo GetRepository(string repository) => Key(repository) switch
    {
        "checkout" => new(repository, "Checkout API and customer order processing."),
        "payments" => new(repository, "Payment processing and settlement."),
        "catalog" => new(repository, "Product catalogue."),
        _ => throw Unknown("repository", repository, "checkout, payments, catalog")
    };

    public static Issue[] GetIssues(string repository) => Key(repository) switch
    {
        "checkout" =>
        [
            new(18, "Checkout timeouts after v2.4.1 connection-pool change", "critical"),
            new(19, "Improve checkout documentation", "low")
        ],
        "payments" => [new(22, "Review release migration approval", "high")],
        "catalog" => [],
        _ => throw Unknown("repository", repository, "checkout, payments, catalog")
    };

    public static Customer GetCustomer(string customerId) => Key(customerId) switch
    {
        "123" => new(customerId, "Alex Example"),
        _ => throw Unknown("customerId", customerId, "123")
    };

    public static Order[] GetOrders(string customerId) => Key(customerId) switch
    {
        "123" => [new("ORD-9001", customerId, "checkout timed out; payment not captured", "checkout")],
        _ => throw Unknown("customerId", customerId, "123")
    };

    public static TestRun GetTests(string project) => Key(project) switch
    {
        "checkout" => new(128, 2, ["CheckoutCompletesUnderLoad", "ConnectionPoolRecoversAfterTimeout"]),
        "payments" => new(94, 0, []),
        _ => throw Unknown("project", project, "checkout, payments")
    };

    public static SecurityFinding[] GetSecurityFindings(string project) => Key(project) switch
    {
        "checkout" => [new("SEC-17", "high", "Dependency advisory requires upgrading the checkout HTTP client.")],
        "payments" => [],
        _ => throw Unknown("project", project, "checkout, payments")
    };

    private static string Key(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToLowerInvariant();
    }

    private static ArgumentException Unknown(string parameter, string value, string allowed) =>
        new($"Unknown {parameter} '{value}'. Known values: {allowed}.", parameter);
}
