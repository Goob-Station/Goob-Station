// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.StatusEffectNew;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class ChangeDamageModiferEffectSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChangeDamageModiferSetStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<ChangeDamageModiferSetStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnApplied(Entity<ChangeDamageModiferSetStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<DamageableComponent>(args.Target, out var damageable))
            return;

        ent.Comp.OriginalDamageModifierSet = damageable.DamageModifierSetId;
        _damageable.SetDamageModifierSetId((args.Target, damageable), ent.Comp.DamageModifierSet);
    }

    private void OnRemoved(Entity<ChangeDamageModiferSetStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!ent.Comp.GoToOriginalOnRemove)
            return;

        _damageable.SetDamageModifierSetId(args.Target, ent.Comp.OriginalDamageModifierSet);
    }
}
