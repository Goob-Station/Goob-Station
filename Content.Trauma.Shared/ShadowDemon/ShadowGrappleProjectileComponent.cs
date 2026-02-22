using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon;


[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowGrappleProjectileComponent : Component
{
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);
}
