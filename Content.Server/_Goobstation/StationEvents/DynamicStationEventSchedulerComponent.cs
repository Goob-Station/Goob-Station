using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents;

[RegisterComponent]
public sealed partial class DynamicStationEventSchedulerComponent : Component
{
    [DataField] public ProtoId<DatasetPrototype>? MidroundRulesPool = null;

    /// <summary>
    ///     Midround rules pool for rolling antag related events.
    /// </summary>
    public List<EntProtoId> MidroundRules = new();
}
