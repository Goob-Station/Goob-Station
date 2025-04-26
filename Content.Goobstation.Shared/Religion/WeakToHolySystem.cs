// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Religion;

public sealed partial class SharedWeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GoobBibleSystem _goobBible = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WeakToHolyComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<WeakToHolyComponent, AfterInteractUsingEvent>(AfterBibleUse);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
    }
    private void OnInit(EntityUid uid, WeakToHolyComponent comp, ref MapInitEvent args)
    {
        // Only change to "BiologicalMetaphysical" if the original damage container was "Biological"
        if (TryComp<DamageableComponent>(uid, out var damageable) && damageable.DamageContainerID == comp.BiologicalContainerId)
            _damageableSystem.ChangeDamageContainer(uid, comp.MetaphysicalContainerId);
    }

    private void AfterBibleUse(Entity<WeakToHolyComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!TryComp<BibleComponent>(args.Used, out var bibleComp)
            || !TryComp(args.Used, out UseDelayComponent? useDelay)
            || _delay.IsDelayed((args.Used, useDelay))
            || !HasComp<BibleUserComponent>(args.User)
            || args.Target is not { } target)
            return;

        _goobBible.TryDoSmite(target, bibleComp, args, useDelay);
    }

    // Passively heal
    private void OnCollide(Entity<HereticRitualRuneComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<WeakToHolyComponent>(args.OtherEntity, out var weak))
            return;

        weak.IsColliding = true;
    }

    private void OnCollideEnd(Entity<HereticRitualRuneComponent> ent, ref EndCollideEvent args)
    {
        if (!TryComp<WeakToHolyComponent>(args.OtherEntity, out var weak))
            return;

        weak.IsColliding = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WeakToHolyComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextHealTick > _timing.CurTime || !comp.IsColliding)
                continue;

            _damageableSystem.TryChangeDamage(uid, comp.HealAmount);

            comp.NextHealTick = _timing.CurTime + comp.HealTickDelay;
        }
    }
}
