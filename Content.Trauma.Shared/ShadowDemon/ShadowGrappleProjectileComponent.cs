using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon;


[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowGrappleProjectileComponent : Component
{
    [DataField]
    public DamageSpecifier DamageOnHit = new();

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float BreakLightsRange = 2f;
}
