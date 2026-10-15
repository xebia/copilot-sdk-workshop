using GitHub.Copilot;
using Microsoft.Extensions.AI;
using System.ComponentModel;

Console.WriteLine("Lab 02: Custom Tool");

// TODO: Define get_service_status(service) with CopilotTool.DefineTool.
// Use Description on its service parameter and AIFunctionFactoryOptions for tool metadata.
// Return structured data: checkout = degraded, payments = healthy, catalog = healthy.
// For unknown names, throw an ArgumentException with the known service names.
// TODO: Register the tool in SessionConfig.Tools and its name in AvailableTools.
// TODO: Subscribe to ToolExecutionStartEvent before sending the prompt.
// TODO: Ask "Why might checkout currently be unavailable?" and print the answer.
// BONUS: Define get_incidents(service), register it, then repeat the question.
