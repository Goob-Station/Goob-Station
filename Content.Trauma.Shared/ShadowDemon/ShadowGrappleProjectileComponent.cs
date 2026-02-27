using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon;


[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowGrappleProjectileComponent : Component
{
    /// <summary>
    /// The damage inflicted on entities when the grapple touches them.
    /// Does not apply to structures
    /// </summary>
    [DataField]
    public DamageSpecifier DamageOnHit = new();

    /// <summary>
    /// How long to stun the entity this projectile touched
    /// </summary>
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The range of the lookup for destroying lights, when the projectile collides with an entity
    /// </summary>
    [DataField]
    public float BreakLightsRange = 2f;
}
