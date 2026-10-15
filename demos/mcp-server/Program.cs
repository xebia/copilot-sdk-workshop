using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workshop.McpServer;

var builder = Host.CreateApplicationBuilder(args);
// Stdout belongs to the MCP protocol. Diagnostics must go to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<ServiceTools>();
await builder.Build().RunAsync();
