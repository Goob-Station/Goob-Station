// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.Pulling.Events;
using Content.Goobstation.Shared._Trauma.Ranching.Components;
using Content.Shared.Damage;

namespace Content.Goobstation.Shared._Trauma.Ranching.Systems;

/// <summary>
/// This handles gibing stuff when its pulled
/// </summary>
public sealed partial class DealDamageOnPulledSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DealDamageOnPulledComponent, BeingPulledAttemptEvent>(OnPullAttempt);
    }

    private void OnPullAttempt(Entity<DealDamageOnPulledComponent> ent, ref BeingPulledAttemptEvent args)
    {
        _damageable.TryChangeDamage(ent.Owner, ent.Comp.Damage, origin: args.Puller);
    }
}
