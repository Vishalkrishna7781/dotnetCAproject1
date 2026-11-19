namespace Sthairya.Models;

/// <summary>One abnormal vital (or pattern) found by the engine.</summary>
public record Finding(string Vital, string Level, string Message);

/// <summary>Engine output: overall level, model score, details and which first-aid card to show.</summary>
public record Assessment(string Level, double ModelScore, List<Finding> Findings, string Guidance);
