// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyItemDamage);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<WeakToHolyComponent, AttackedEvent>(OnDamageTaken);
    }

    private void OnStartup(EntityUid uid, WeakToHolyComponent comp, ref MapInitEvent args)
    {
        // Only change to "BiologicalMetaphysical" if the original damage container was "Biological"
        if (TryComp<DamageableComponent>(uid, out var damageable) && damageable.DamageContainerID == comp.BiologicalContainerId)
            _damageableSystem.ChangeDamageContainer(uid, comp.MetaphysicalContainerId);
    }

    #region Holy Damage Dealing

    private void OnUnholyItemDamage(EntityUid uid, WeakToHolyComponent comp, ref DamageUnholyEvent args)
    {
        if (comp.AlwaysTakeHoly)
        {
            args.ShouldTakeHoly = true;
            return;
        }

        if (_inventorySystem.GetHandOrInventoryEntities(args.Target, SlotFlags.All & SlotFlags.POCKET).Any(HasComp<UnholyItemComponent>))
            args.ShouldTakeHoly = true; // may allah forgive me for this linq :pray:
    }

    private void OnDamageTaken(Entity<WeakToHolyComponent> ent, ref AttackedEvent args)
    {
        if (!TryComp<NullrodComponent>(args.Used, out var weapon) || weapon.HolyDamage is null)
            return;

        var unholyEvent = new DamageUnholyEvent(ent, weapon.HolyDamage, args.Used);
        RaiseLocalEvent(ent, unholyEvent);

        if (unholyEvent.ShouldTakeHoly)
            _damageableSystem.TryChangeDamage(unholyEvent.Target, unholyEvent.Damage, origin: unholyEvent.Origin);
    }

    #endregion

    #region Heretic Rune Healing

    // Passively heal on runes
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

    #endregion
}
