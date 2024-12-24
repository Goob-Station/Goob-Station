using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TwistedConstructionTargetComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ReplacementProto = "";

    [DataField]
    public TimeSpan DoAfterDelay = TimeSpan.FromSeconds(2);
}
