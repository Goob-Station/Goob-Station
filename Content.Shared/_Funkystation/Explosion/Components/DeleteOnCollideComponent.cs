using Content.Shared._Funkystation.Explosion.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Explosion.Components;

/// <summary>
///     Deletes this entity when it hits an obstacle
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ExplosionProjectileCollisionSystem))]
public sealed partial class DeleteOnCollideComponent : Component;
