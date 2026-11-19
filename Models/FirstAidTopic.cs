namespace Sthairya.Models;

/// <summary>One first-aid card. Id matches the Guidance value returned by the engine.</summary>
public record FirstAidTopic(string Id, string Title, string Signs, IReadOnlyList<string> Steps);
