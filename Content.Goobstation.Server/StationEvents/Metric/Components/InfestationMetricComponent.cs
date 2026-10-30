using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(InfestationMetricSystem))]
public sealed partial class InfestationMetricComponent : Component
{
    [DataField]
    public double KudzuCost = 0.25;

    [DataField]
    public double VerminCost = 2.0;

    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> VerminFactions = new()
    {
        "Mouse",
        "SimpleNeutral",
    };
}
