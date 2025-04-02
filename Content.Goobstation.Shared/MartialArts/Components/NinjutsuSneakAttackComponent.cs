using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NinjutsuSneakAttackComponent : Component
{
    [DataField]
    public float Multiplier = 2f;

    [DataField]
    public float AssassinateModifier = 50f;

    [DataField]
    public SoundSpecifier AssassinateSoundUnarmed = new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg");

    [DataField]
    public SoundSpecifier AssassinateSoundArmed =
            new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/guillotine.ogg");

    // This should be LocId but combos names don't use locale anyway
    [DataField]
    public string AssassinateComboName = "Assassinate";

    [DataField]
    public ProtoId<AlertPrototype> Alert = "SneakAttack";
}
