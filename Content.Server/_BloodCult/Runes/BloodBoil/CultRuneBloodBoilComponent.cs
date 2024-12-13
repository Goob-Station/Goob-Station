using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.BloodBoil;

[RegisterComponent]
public sealed partial class CultRuneBloodBoilComponent : Component
{
    [DataField]
    public EntProtoId ProjectilePrototype = "BloodBoilProjectile";

    [DataField]
    public float ProjectileSpeed = 50;

    [DataField]
    public float TargetsLookupRange = 15f;

    [DataField]
    public float ProjectileCount = 3;

    [DataField]
    public float FireStacksPerProjectile = 1;

    [DataField]
    public SoundSpecifier ActivationSound = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/magic.ogg");
}
