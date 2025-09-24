using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.Weapons;

/// <summary>
/// This handles Prevention of usage of guns.
/// </summary>
public sealed class PreventGunUseSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PreventGunUseComponent,ShotAttemptedEvent>(OnGunUse);
    }

    private void OnGunUse(Entity<PreventGunUseComponent> ent,ref ShotAttemptedEvent arg)
    {
        arg.Cancel();
    }
}
