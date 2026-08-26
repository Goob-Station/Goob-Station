using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Charge up, dash, break first structure hit or knock down and damage first living thing hit.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorChargeComponent : Component
{
    [DataField]
    public float DashDistance = 4f;

    [DataField]
    public float DashSpeed = 6f;

    [DataField]
    public DamageSpecifier StructureDamage = new();

    [DataField]
    public DamageSpecifier TargetDamage = new();

    [DataField]
    public TimeSpan TargetStun = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan TargetKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier? ChargeSound;

    [DataField]
    public SoundSpecifier? ImpactSound;
    public bool IsCharging;
}
