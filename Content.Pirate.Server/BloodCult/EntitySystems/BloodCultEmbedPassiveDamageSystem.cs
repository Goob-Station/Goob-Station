// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Timing;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodCultEmbedPassiveDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultEmbedPassiveDamageComponent, EmbedEvent>(OnEmbed);
        SubscribeLocalEvent<BloodCultEmbedPassiveDamageComponent, EmbedDetachEvent>(OnEmbedDetach);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<BloodCultEmbedPassiveDamageComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Embedded is not { } embedded)
                continue;

            if (!Exists(embedded) ||
                !HasComp<DamageableComponent>(embedded) ||
                !HasComp<MobStateComponent>(embedded) ||
                _mobState.IsDead(embedded))
            {
                Reset(component);
                continue;
            }

            if (component.NextDamage > currentTime)
                continue;

            component.NextDamage = currentTime + component.DamageInterval;
            _damageable.TryChangeDamage(
                embedded,
                component.Damage,
                interruptsDoAfters: false,
                origin: uid);
        }
    }

    private void OnEmbed(Entity<BloodCultEmbedPassiveDamageComponent> item, ref EmbedEvent args)
    {
        if (!HasComp<DamageableComponent>(args.Embedded) || !HasComp<MobStateComponent>(args.Embedded))
            return;

        if (item.Comp.Damage.Empty || item.Comp.Damage.GetTotal() <= 0)
            return;

        item.Comp.Embedded = args.Embedded;
        item.Comp.NextDamage = _timing.CurTime + item.Comp.DamageInterval;
    }

    private void OnEmbedDetach(Entity<BloodCultEmbedPassiveDamageComponent> item, ref EmbedDetachEvent args)
    {
        if (item.Comp.Embedded == args.Embedded)
            Reset(item.Comp);
    }

    private static void Reset(BloodCultEmbedPassiveDamageComponent component)
    {
        component.Embedded = null;
        component.NextDamage = TimeSpan.Zero;
    }
}
