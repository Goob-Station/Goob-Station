using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandChargeComponent : Component
{
    [DataField]
    public EntProtoId LandingProto = "BubblegumLanding";

    [DataField]
    public float ChargeSpeed = 20f;

    public bool IsCharging;

    [DataField]
    public TimeSpan ChargeDelay = TimeSpan.FromSeconds(4);

    public TimeSpan NextChargeTime;

    [DataField]
    public float LandingReachDistance = 0.5f;

    [DataField]
    public float TargetSearchRange = 20f;

    [DataField]
    public EntityUid? Landing;

    [DataField]
    public EntityWhitelist? TargetWhitelist = new();

    [DataField]
    public EntityWhitelist? TargetBlacklist = new();
}

