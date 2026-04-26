using Content.Goobstation.Server.StationEvents.Events;

namespace Content.Goobstation.Server.StationEvents.Components;

/// <summary>
/// Configuration for the manhunt station event.
/// The event scans all entities with a notoriety component and, if enough notorious
/// criminals are present, broadcasts their names and bounties over station comms.
/// </summary>
[RegisterComponent]
[Access(typeof(ManhuntRule))]
public sealed partial class ManhuntRuleComponent : Component
{
    /// <summary>
    /// Minimum notoriety level a criminal must have to be included in the announcement.
    /// </summary>
    [DataField]
    public int MinNotorietyLevel = 3;

    /// <summary>
    /// Minimum sum of notoriety levels across all qualifying criminals required for the
    /// event to fire. If the total is below this the event aborts immediately.
    /// </summary>
    [DataField]
    public int MinTotalNotoriety = 8;

    /// <summary>
    /// Maximum number of criminals to list in the announcement, sorted by level descending.
    /// </summary>
    [DataField]
    public int MaxListed = 5;
}
