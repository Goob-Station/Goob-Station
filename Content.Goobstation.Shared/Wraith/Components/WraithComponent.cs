using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Abilities;

    [DataField]
    public EntProtoId WraithWeakenedEffect = "StatusEffectWeakenedWraith";

    [DataField]
    public EntProtoId WraithDeathEffect = "WraithDeathEffect";
}
