using System.ComponentModel;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

namespace Workshop.Tools;

public static class ToolCatalog
{
    public static AIFunction BuildStatus() => Define(
        ([Description("Project: frontend, api, database, payments, or checkout.")] string project) =>
            WorkshopData.GetBuildStatus(project),
        "get_build_status", "Gets the current build status. A passing build alone does not imply release readiness.");

    public static AIFunction ServiceStatus() => Define(
        ([Description("Service: checkout, payments, or catalog.")] string service) =>
            WorkshopData.GetServiceStatus(service),
        "get_service_status", "Gets service health. Use get_incidents to investigate a degraded service.");

    public static AIFunction Incidents() => Define(
        ([Description("Service: checkout, payments, or catalog.")] string service) =>
            WorkshopData.GetIncidents(service),
        "get_incidents", "Lists current incidents and investigation findings for a service.");

    public static AIFunction DeploymentStatus() => Define(
        ([Description("Service: checkout, payments, or catalog.")] string service) =>
            WorkshopData.GetDeploymentStatus(service),
        "get_deployment_status", "Gets the deployment status and reason it is blocked or degraded.");

    public static AIFunction PullRequests() => Define(
        ([Description("Repository: checkout, payments, or catalog.")] string repository) =>
            WorkshopData.GetOpenPullRequests(repository),
        "get_open_pull_requests", "Lists open pull requests, including whether each one blocks release.");

    public static AIFunction Repository() => Define(
        ([Description("Repository: checkout, payments, or catalog.")] string repository) =>
            WorkshopData.GetRepository(repository),
        "get_repository", "Gets a repository's purpose. This is fictional workshop data, not a live GitHub lookup.");

    public static AIFunction Issues() => Define(
        ([Description("Repository: checkout, payments, or catalog.")] string repository) =>
            WorkshopData.GetIssues(repository),
        "get_issues", "Lists open issues and their priority for a repository.");

    public static AIFunction Customer() => Define(
        ([Description("Customer ID. The workshop dataset contains 123.")] string customerId) =>
            WorkshopData.GetCustomer(customerId),
        "get_customer", "Gets a fictional customer's profile.");

    public static AIFunction Orders() => Define(
        ([Description("Customer ID. The workshop dataset contains 123.")] string customerId) =>
            WorkshopData.GetOrders(customerId),
        "get_orders", "Lists a customer's orders, status and affected service.");

    public static AIFunction Tests() => Define(
        ([Description("Project: checkout or payments.")] string project) => WorkshopData.GetTests(project),
        "get_tests", "Gets test pass/fail counts and failing test names for release evaluation.");

    public static AIFunction SecurityFindings() => Define(
        ([Description("Project: checkout or payments.")] string project) =>
            WorkshopData.GetSecurityFindings(project),
        "get_security_findings", "Lists open security findings with severity. An empty list means no known findings.");

    public static AIFunction[] ForTrack(string track) => track.ToLowerInvariant() switch
    {
        "devops" => [ServiceStatus(), BuildStatus(), DeploymentStatus(), Incidents()],
        "github" => [Repository(), PullRequests(), Issues()],
        "support" => [Customer(), Orders(), Incidents()],
        "release" => [BuildStatus(), Tests(), PullRequests(), SecurityFindings()],
        _ => throw new ArgumentException("Track must be devops, github, support, or release.", nameof(track))
    };

    private static AIFunction Define(Delegate handler, string name, string description) =>
        CopilotTool.DefineTool(handler, factoryOptions: new AIFunctionFactoryOptions
        {
            Name = name,
            Description = description
        });
}
