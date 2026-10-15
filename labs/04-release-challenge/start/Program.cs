using GitHub.Copilot;
using Workshop.Tools;

Console.WriteLine("Final challenge: Are we ready to release?");
var tools = ToolCatalog.ForTrack("release");

// The four read-only tools are provided so this stays a 10-minute exercise.
// TODO: Create a session with these tools and an explicit AvailableTools list.
// TODO: Ask whether checkout is ready to release and request an evidence-based answer.
// TODO: Observe tool calls. Does the answer consider tests and security as well as the build?
// BONUS: Try payments. A clean build/test run still does not remove its blocking pull request.
