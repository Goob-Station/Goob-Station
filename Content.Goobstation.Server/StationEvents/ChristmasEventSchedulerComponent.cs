using Content.Shared.Destructible.Thresholds;

namespace Content.Goobstation.Server.StationEvents;

[RegisterComponent]
public sealed partial class ChristmasEventSchedulerComponent : Component
{
    /// <summary>
    ///     How long until the next check for an event runs
    /// </summary>
    [DataField] public float EventClock = 600f; // Ten minutes

    /// <summary>
    ///     How much time it takes in seconds for an antag event to be raised.
    /// </summary>
    [DataField] public MinMax Delays = new(5 * 60, 30 * 60);
}
