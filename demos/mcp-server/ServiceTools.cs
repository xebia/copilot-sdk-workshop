using System.ComponentModel;
using ModelContextProtocol.Server;
using Workshop.Tools;

namespace Workshop.McpServer;

[McpServerToolType]
public sealed class ServiceTools
{
    [McpServerTool(Name = "get_service_status"), Description("Gets service health from fictional workshop data.")]
    public static ServiceStatus GetServiceStatus(
        [Description("Service: checkout, payments, or catalog.")] string service) =>
        WorkshopData.GetServiceStatus(service);

    [McpServerTool(Name = "get_incidents"), Description("Lists current incidents and investigation findings.")]
    public static Incident[] GetIncidents(
        [Description("Service: checkout, payments, or catalog.")] string service) =>
        WorkshopData.GetIncidents(service);
}
