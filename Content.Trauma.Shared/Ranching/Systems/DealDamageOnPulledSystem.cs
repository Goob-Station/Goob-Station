// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.Pulling.Events;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

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
        _damageable.ChangeDamage(ent.Owner, ent.Comp.Damage, origin: args.Puller);
    }
}
