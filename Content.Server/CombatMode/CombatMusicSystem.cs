// SPDX-License-Identifier: AGPL-3.0-or-later

// Provides the shared damage event sent to the affected client.
using Content.Shared.CombatMode;
// Provides DamageableComponent and DamageChangedEvent.
using Content.Shared.Damage;
// Provides ActorComponent, which identifies entities controlled by a player session.
using Robust.Shared.Player;

namespace Content.Server.CombatMode;

/// <summary>
/// Detects authoritative player damage and notifies only the client controlling that entity.
/// </summary>
public sealed class CombatMusicSystem : EntitySystem
{
    /// <summary>
    /// Connects the server's damage event to the targeted combat-music notification.
    /// </summary>
    public override void Initialize()
    {
        // Runs the normal EntitySystem setup first.
        base.Initialize();

        // Observes final damage changes after modifiers such as armor have been applied.
        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    /// <summary>
    /// Sends a music trigger when a player-controlled entity actually gains damage.
    /// </summary>
    private void OnDamageChanged(Entity<DamageableComponent> entity, ref DamageChangedEvent args)
    {
        // Ignores healing, unchanged damage, and entities that are not currently controlled by a player.
        if (args.DamageDelta == null ||
            !args.DamageIncreased ||
            !TryComp(entity, out ActorComponent? actor))
        {
            return;
        }

        // Targets only the damaged player's session so other clients do not start combat music.
        RaiseNetworkEvent(new CombatMusicDamageEvent(), actor.PlayerSession);
    }
}
