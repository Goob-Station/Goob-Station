using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.RadialSelector;
using Content.Shared.Actions.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.Evolution;

[RegisterComponent]
public sealed partial class XenomorphEvolutionComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> EvolvesTo = new();

    [DataField]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);

    [DataField]
    public FixedPoint2 Points;

    [DataField]
    public FixedPoint2 Max;

    [DataField]
    public FixedPoint2 PointsPerSecond = 2;

    [DataField]
    public TimeSpan EvolutionJitterDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public EntProtoId<InstantActionComponent> EvolutionActionId = "ActionEvolution";

    [ViewVariables]
    public EntityUid? EvolutionAction;

    [ViewVariables]
    public TimeSpan NextPointsAt;
}
