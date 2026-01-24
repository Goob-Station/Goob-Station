using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bubblegum.Components;

[RegisterComponent]
public sealed partial class BubblegumChargeComponent : Component
{
    [DataField]
    public EntProtoId LandingProto = "BubblegumLanding";

    [DataField]
    public float ChargeSpeed = 20f;

    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public bool IsCharging;

    [DataField]
    public TimeSpan ChargeDelay = TimeSpan.FromSeconds(3);

    public TimeSpan NextChargeTime;

    [DataField]
    public float LandingReachDistance = 0.5f;

    [DataField]
    public float TargetSearchRange = 20f;

    public EntityUid? Landing;
}


