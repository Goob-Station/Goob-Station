using Content.Server.GameTicking.Rules;
using Content.Shared.Dataset;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents;

[RegisterComponent]
public sealed partial class DynamicStationEventSchedulerComponent : Component
{
    [DataField] public ProtoId<DatasetPrototype> MidroundRulesPool;
    public List<EntProtoId> ExecutedRules = new();

    /// <summary>
    /// How long until the next check for an event runs, is initially set based on MinimumTimeUntilFirstEvent & MinMaxEventTiming.
    /// </summary>
    [DataField] public float EventClock = 1200f; // = FirstEventDelay

    [DataField] public float? Budget = null;

    /// <summary>
    ///     How much time it takes in seconds for an antag event to be raised. (min)
    /// </summary>
    /// <remarks>Default is 10 minutes</remarks>
    [DataField] public MinMax Delays = new(10 * 60, 20 * 60);

    /// <summary>
    ///     The first midround antag roll will happen 20 minutes into the shift.
    ///     After that it's all about random.
    /// </summary>
    [DataField] public float FirstEventDelay = 1200f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float ThreatPerMidroundRoll = 7f;
}
