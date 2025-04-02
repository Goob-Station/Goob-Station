using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent]
public sealed partial class TemporalSlashComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public int HitsLeft = 2;

    [DataField]
    public float HitDelay = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;

    [DataField]
    public EntProtoId Effect = "WeaponArcTempSlash";

    [DataField]
    public SoundSpecifier? HitSound = new SoundPathSpecifier("/Audio/Weapons/bladeslice.ogg");
}
