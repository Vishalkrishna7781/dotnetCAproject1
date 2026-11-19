namespace Sthairya.Models;

public record ScenarioOption(string Value, string Label);
public record StatusItem(string Label, string Tag);

/// <summary>Data the dashboard view needs. The controller builds it; the view only displays it.</summary>
public class DashboardViewModel
{
    public required IReadOnlyList<ScenarioOption> Scenarios { get; init; }
    public required IReadOnlyList<StatusItem> Status { get; init; }

    public static DashboardViewModel Create() => new()
    {
        Scenarios = new[]
        {
            new ScenarioOption("normal",  "Simulated: normal"),
            new ScenarioOption("heat",    "Simulated: heat stress"),
            new ScenarioOption("hypoxia", "Simulated: low oxygen"),
            new ScenarioOption("shock",   "Simulated: shock after injury"),
            new ScenarioOption("manual",  "Manual entry"),
        },
        Status = new[]
        {
            new StatusItem("AI inference",   "LOCAL"),
            new StatusItem("Network needed", "NO"),
            new StatusItem("Data shared",    "NONE"),
        }
    };
}
