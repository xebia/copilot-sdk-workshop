using GitHub.Copilot;
using Workshop.Lab03;
using Xunit;

namespace Workshop.Tests;

public sealed class Lab03Tests
{
    private static string AgentDirectory => Path.Combine(AppContext.BaseDirectory, "lab3-agents");

    [Fact]
    public void Configuration_exposes_dispatch_and_scoped_domain_tools_but_excludes_domain_tools_from_parent()
    {
        var config = Investigation.CreateConfiguration(AgentDirectory);
        var domainTools = Investigation.SpecialistTools.Values.SelectMany(names => names).Order().ToArray();
        Assert.Equal(domainTools, config.Tools!.Select(tool => tool.Name).Order());
        Assert.Equal(domainTools, config.DefaultAgent!.ExcludedTools!.Order());
        Assert.Equal(domainTools.Append("task").Order(), config.AvailableTools!.Order());
        Assert.Equal(Path.GetFullPath(AgentDirectory), Assert.Single(config.AgentDirectories!));
        Assert.False(config.EnableConfigDiscovery);
        Assert.False(config.EnableSkills);
        Assert.Null(config.CustomAgents);
    }

    [Fact]
    public void Missing_agent_directory_fails_explicitly() =>
        Assert.Throws<DirectoryNotFoundException>(() =>
            Investigation.CreateConfiguration(Path.Combine(AgentDirectory, Guid.NewGuid().ToString())));

    [Fact]
    public void Missing_and_empty_definition_files_fail_explicitly()
    {
        var directory = Directory.CreateTempSubdirectory("lab3-definitions-");
        try
        {
            Assert.Throws<FileNotFoundException>(() => Investigation.CreateConfiguration(directory.FullName));
            File.WriteAllText(Path.Combine(directory.FullName, "customer-investigator.agent.md"), "");
            Assert.Throws<InvalidDataException>(() => Investigation.CreateConfiguration(directory.FullName));
        }
        finally { directory.Delete(recursive: true); }
    }

#pragma warning disable GHCP001
    [Fact]
    public async Task Native_runtime_discovers_packaged_files_and_enforces_each_tool_scope_without_model_calls()
    {
        var config = Investigation.CreateConfiguration(AgentDirectory);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await using var client = new CopilotClient();
        await using var session = await client.CreateSessionAsync(config, timeout.Token);
        await Investigation.ValidateDiscoveryAsync(session, AgentDirectory, timeout.Token);
        foreach (var (name, expectedTools) in Investigation.SpecialistTools)
        {
            await session.Rpc.Agent.SelectAsync(name, timeout.Token);
            await session.Rpc.Tools.InitializeAndValidateAsync(timeout.Token);
            var metadata = await session.Rpc.Tools.GetCurrentMetadataAsync(timeout.Token);
            Assert.NotNull(metadata.Tools);
            Assert.Equal(expectedTools.Order(), metadata.Tools.Select(tool => tool.Name).Order());
        }
        await session.Rpc.Agent.DeselectAsync(timeout.Token);
        await Investigation.ValidateDiscoveryAsync(session, AgentDirectory, timeout.Token);
    }

    [Fact]
    public async Task Native_discovery_rejects_an_incorrect_specialist_scope_without_model_calls()
    {
        var directory = Directory.CreateTempSubdirectory("lab3-invalid-scope-");
        try
        {
            foreach (var file in Directory.GetFiles(AgentDirectory, "*.agent.md"))
                File.Copy(file, Path.Combine(directory.FullName, Path.GetFileName(file)));
            var customer = Path.Combine(directory.FullName, "customer-investigator.agent.md");
            File.WriteAllText(customer, File.ReadAllText(customer).Replace(
                "[\"get_customer\", \"get_orders\"]", "[\"get_incidents\"]", StringComparison.Ordinal));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await using var client = new CopilotClient();
            await using var session = await client.CreateSessionAsync(
                Investigation.CreateConfiguration(directory.FullName), timeout.Token);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Investigation.ValidateDiscoveryAsync(session, directory.FullName, timeout.Token));
        }
        finally { directory.Delete(recursive: true); }
    }
#pragma warning restore GHCP001

    [Fact]
    public void An_idle_session_without_reports_is_not_success() =>
        Assert.Throws<InvalidOperationException>(() => new DelegationTrace(TextWriter.Null).EnsureComplete());

    [Fact]
    public void Both_specialists_must_finish_with_matching_start_events()
    {
        var trace = new DelegationTrace(TextWriter.Null);
        foreach (var name in Investigation.SpecialistTools.Keys.Reverse())
        {
            trace.Record(Started(name));
            trace.Record(Completed(name));
        }
        trace.EnsureComplete();
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("cancelled")]
    [InlineData("running")]
    [InlineData("missing-start")]
    public void Failed_cancelled_running_or_unmatched_work_is_not_a_successful_report(string state)
    {
        using var output = new StringWriter();
        var trace = new DelegationTrace(output);
        const string customer = "customer-investigator";
        if (state != "missing-start") trace.Record(Started(customer));
        switch (state)
        {
            case "failed":
                trace.Record(new SubagentFailedEvent
                {
                    Data = new SubagentFailedData
                    {
                        AgentName = customer, AgentDisplayName = customer, ToolCallId = customer,
                        Error = "Fixture failure"
                    }
                });
                Assert.Contains("Fixture failure", output.ToString());
                break;
            case "cancelled":
                var cancelled = Completed(customer);
                cancelled.Data.Cancelled = true;
                trace.Record(cancelled);
                break;
            case "missing-start":
                trace.Record(Completed(customer));
                break;
        }
        trace.Record(Started("service-investigator"));
        trace.Record(Completed("service-investigator"));
        Assert.Throws<InvalidOperationException>(trace.EnsureComplete);
    }

    private static SubagentStartedEvent Started(string name) => new()
    {
        Data = new SubagentStartedData
        {
            AgentName = name, AgentDisplayName = name, AgentDescription = "Workshop specialist", ToolCallId = name
        }
    };

    private static SubagentCompletedEvent Completed(string name) => new()
    {
        Data = new SubagentCompletedData { AgentName = name, AgentDisplayName = name, ToolCallId = name }
    };
}
