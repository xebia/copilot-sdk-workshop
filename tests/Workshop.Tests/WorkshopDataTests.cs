using Workshop.Tools;
using Xunit;

namespace Workshop.Tests;

public sealed class WorkshopDataTests
{
    [Theory]
    [InlineData("frontend", "passing")]
    [InlineData("api", "failing")]
    [InlineData("database", "passing")]
    [InlineData("payments", "passing")]
    [InlineData("checkout", "passing")]
    [InlineData(" CHECKOUT ", "passing")]
    public void Build_status_matches_the_slides(string project, string expected) =>
        Assert.Equal(expected, WorkshopData.GetBuildStatus(project).Status);

    [Theory]
    [InlineData("checkout", "degraded")]
    [InlineData("payments", "healthy")]
    [InlineData("catalog", "healthy")]
    public void Service_status_matches_lab_two(string service, string expected) =>
        Assert.Equal(expected, WorkshopData.GetServiceStatus(service).Status);

    [Fact]
    public void Checkout_incident_links_to_deployed_version()
    {
        var deployment = WorkshopData.GetDeploymentStatus("checkout");
        var incident = Assert.Single(WorkshopData.GetIncidents("checkout"));
        Assert.Contains("v2.4.1", deployment.Reason);
        Assert.Contains("v2.4.1", incident.Summary);
        Assert.Equal("investigating", incident.Status);
    }

    [Fact]
    public void Healthy_services_have_no_incidents()
    {
        Assert.Empty(WorkshopData.GetIncidents("payments"));
        Assert.Empty(WorkshopData.GetIncidents("catalog"));
    }

    [Fact]
    public void Payments_deployment_is_blocked_despite_passing_build()
    {
        Assert.Equal("passing", WorkshopData.GetBuildStatus("payments").Status);
        Assert.Equal("blocked", WorkshopData.GetDeploymentStatus("payments").Status);
        var pr = Assert.Single(WorkshopData.GetOpenPullRequests("payments"));
        Assert.Equal(42, pr.Number);
        Assert.True(pr.BlocksRelease);
    }

    [Fact]
    public void Support_evidence_can_be_joined_by_customer_and_service()
    {
        var customer = WorkshopData.GetCustomer("123");
        var order = Assert.Single(WorkshopData.GetOrders(customer.Id));
        var incident = Assert.Single(WorkshopData.GetIncidents(order.Service));
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal("checkout", incident.Service);
        Assert.Contains("not captured", order.Status);
    }

    [Fact]
    public void GitHub_track_exposes_a_priority_and_related_fix()
    {
        Assert.Contains("Checkout", WorkshopData.GetRepository("checkout").Description);
        Assert.Contains(WorkshopData.GetIssues("checkout"), issue => issue.Priority == "critical");
        Assert.Contains(WorkshopData.GetOpenPullRequests("checkout"), pr =>
            pr.Title.Contains("connection-pool") && pr.BlocksRelease);
    }

    [Fact]
    public void Release_challenge_has_independent_test_and_security_blockers()
    {
        Assert.Equal("passing", WorkshopData.GetBuildStatus("checkout").Status);
        var tests = WorkshopData.GetTests("checkout");
        Assert.Equal(2, tests.Failed);
        Assert.Equal(tests.Failed, tests.Failures.Length);
        Assert.Contains(WorkshopData.GetSecurityFindings("checkout"), finding => finding.Severity == "high");
        Assert.Contains(WorkshopData.GetOpenPullRequests("checkout"), pr => pr.BlocksRelease);
        Assert.Equal(0, WorkshopData.GetTests("payments").Failed);
        Assert.Empty(WorkshopData.GetSecurityFindings("payments"));
    }

    [Fact]
    public void Unknown_identifiers_are_errors_not_fake_healthy_results()
    {
        Action[] queries =
        [
            () => WorkshopData.GetBuildStatus("missing"),
            () => WorkshopData.GetServiceStatus("missing"),
            () => WorkshopData.GetIncidents("missing"),
            () => WorkshopData.GetDeploymentStatus("missing"),
            () => WorkshopData.GetOpenPullRequests("missing"),
            () => WorkshopData.GetRepository("missing"),
            () => WorkshopData.GetIssues("missing"),
            () => WorkshopData.GetCustomer("missing"),
            () => WorkshopData.GetOrders("missing"),
            () => WorkshopData.GetTests("missing"),
            () => WorkshopData.GetSecurityFindings("missing")
        ];
        foreach (var query in queries)
            Assert.Contains("Known values:", Assert.Throws<ArgumentException>(query).Message);
        Assert.Throws<ArgumentException>(() => WorkshopData.GetServiceStatus(" "));
    }
}
