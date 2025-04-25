using Content.Shared.Damage;

namespace Content.Goobstation.Server.Power.PTL;

[RegisterComponent]
public sealed partial class PTLComponent : Component
{
    [DataField] public bool Active = false;

    [DataField] public float SpesosHeld = 0f;

    [DataField] public float MinShootPower = (float) 10e6; // 1 MJ
    [DataField] public float MaxEnergyPerShot = (float) 10e9; // 1 GJ

    [DataField] public TimeSpan ShootDelay = TimeSpan.FromSeconds(10);
    [ViewVariables(VVAccess.ReadOnly)] public TimeSpan NextShotAt = TimeSpan.Zero;

    [DataField] public DamageSpecifier BaseBeamDamage;

    /// <summary>
    ///     Amount of power required to start emitting radiation and blinding people that come nearby
    /// </summary>
    [DataField] public float PowerEvilThreshold = (float) (50 * 10e6) // 50 MJ;
}
