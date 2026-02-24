namespace Content.Goobstation.Common.Body;

/// <summary>
/// Used to see if the entity consciousness/control should be removed from the body.
/// </summary>
/// <param name="Blocked"> Should brain control/consciousness transfer be blocked?. </param>
[ByRefEvent]
public record struct BeforeBrainRemovedEvent(
    bool Blocked = false);

/// <summary>
/// Used to see if the entity consciousness/control should be transferred into the body.
/// </summary>
/// <param name="Blocked"> Should brain control/consciousness transfer be blocked?. </param>
[ByRefEvent]
public record struct BeforeBrainAddedEvent(
    bool Blocked = false);
