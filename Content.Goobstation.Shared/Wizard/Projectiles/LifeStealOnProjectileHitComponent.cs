using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class LifeStealOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist;

    [DataField]
    public FixedPoint2 LifeStealAmount = 20;

    [DataField]
    public FixedPoint2 BloodStealAmount = 25;

    [DataField]
    public EntProtoId Effect = "SanguineBloodEffect";
}
