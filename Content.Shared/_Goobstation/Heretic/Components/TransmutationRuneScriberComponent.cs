using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TransmutationRuneScriberComponent : Component
{
    [DataField]
    public float Time = 5f;

    [DataField]
    public EntProtoId RuneDrawingEntity = "HereticRuneRitualDrawAnimationCicatrix";
}
