using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

/// <summary>
/// Add this and TrailComponent to an entity so that it spawns a trail of that entity sprite.
/// TrailComponent's ParticleAmount should be set to zero for it to work correctly.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EntityTrailComponent : Component
{
}
