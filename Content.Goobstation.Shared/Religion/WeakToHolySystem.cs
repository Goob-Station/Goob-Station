// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
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

        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyItemDamage);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnDamageModify);

    }

    #region Holy Damage Dealing

    private void OnDamageModify(EntityUid uid, DamageableComponent component, DamageModifyEvent args)
    {
        var unholyEvent = new DamageUnholyEvent(args.Target, args.Origin);
        RaiseLocalEvent(args.Target, ref unholyEvent);

        var holyCoefficient = 0f; // Default resistance

        if (unholyEvent.ShouldTakeHoly)
            holyCoefficient = 1f; //Allow holy damage

        DamageModifierSet modifierSet = new()
        {
            Coefficients = new Dictionary<string, float>
            {
                { "Holy", holyCoefficient },
            },
        };

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifierSet);
    }

    private void OnUnholyItemDamage(Entity<WeakToHolyComponent> uid, ref DamageUnholyEvent args)
    {
        if (uid.Comp.AlwaysTakeHoly)
        {
            args.ShouldTakeHoly = true;
            return;
        }

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(args.Target, SlotFlags.All & ~SlotFlags.POCKET))
        {
            if (!HasComp<UnholyItemComponent>(item))
                continue;

            args.ShouldTakeHoly = true;
            return;
        }
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
