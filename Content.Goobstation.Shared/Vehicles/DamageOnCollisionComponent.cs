using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Vehicles.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnCollisionComponent : Component
{
    [DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

    [DataField("sound")]
    public SoundSpecifier? Sound;

    [DataField("minImpactSpeed")]
    public float MinImpactSpeed = 5f;

    [DataField("damageCooldown")]
    public float DamageCooldown = 0.5f;

    [DataField("lastHit", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? LastHit;
}
