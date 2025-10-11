using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MakeRevenantComponent : Component
{
    [DataField]
    public SoundSpecifier? PossessSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/reventer.ogg");

    [DataField]
    public SoundSpecifier? PossessEndSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/revleave.ogg");

    [DataField]
    public DamageSpecifier PassiveRevenantDamage = new();
}
