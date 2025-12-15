namespace Content.Goobstation.Common.Tools;

/// <summary>
/// Event raised on a tool in `SharedToolSystem.UseTool()` if its doAfter timer successfully started.
/// </summary>
/// <param name="User">The entity using the tool.</param>
/// <param name="Target">The entity that the tool is being used on.</param>
public record struct UseToolEvent(EntityUid User, EntityUid? Target);
