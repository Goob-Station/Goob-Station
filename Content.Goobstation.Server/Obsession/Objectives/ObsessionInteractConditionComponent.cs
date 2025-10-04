using Content.Goobstation.Shared.Obsession;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Obsession;

[RegisterComponent, Access(typeof(ObsessionObjectivesSystem))]
public sealed partial class ObsessionInteractConditionComponent : Component
{
    [DataField]
    public MinMax Limits = new(10, 25);

    [DataField]
    public ObsessionInteraction Interaction = ObsessionInteraction.Touch;

    [DataField]
    public EntProtoId? NextObjective;

    [DataField]
    public string Name = "";

    [DataField]
    public string Desc = "";

    public int Required = 0;

    public float? LockedState = null;
}
