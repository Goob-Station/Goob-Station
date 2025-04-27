// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Religion.Events;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
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

        SubscribeLocalEvent<WeakToHolyComponent, DamageModifyEvent>(OnDamageTaken);

    }

    #region Holy Damage Dealing

    private static Dictionary<string, float> _resistanceCoefficientDict = new()
    {
        { "Holy", 1 },
    };

    // Create a modifier set
    DamageModifierSetPrototype holymodifierSet = new()
    {
        Coefficients = _resistanceCoefficientDict
    };

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

    private void OnDamageTaken(EntityUid uid, WeakToHolyComponent comp, DamageModifyEvent args)
    {
        var unholyEvent = new DamageUnholyEvent(args.Target, args.Origin);
        RaiseLocalEvent(args.Target, unholyEvent);

        if (unholyEvent.ShouldTakeHoly)
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, holymodifierSet);
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
