using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Projectiles;

/// <summary>
/// Goob - added to projectiles and hitscans fired by a gun, storing whoever fired it.
/// Their client predicted the shot, so it predicts the server's projectile too and
/// the server excludes them from effects they already played locally.
/// Unlike <see cref="Content.Shared.Projectiles.ProjectileComponent.Shooter"/> this never changes from reflecting.
/// </summary>
/// <remarks>
/// Networked so the shooter's client starts predicting the projectile as soon as it receives it,
/// a separate network event gets handled after that frame's prediction and can arrive before the entity does.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PredictedShooterComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Shooter;
}
