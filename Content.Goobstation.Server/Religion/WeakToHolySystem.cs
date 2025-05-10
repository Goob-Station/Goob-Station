// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GoobBibleSystem _goobBible = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyItemDamage);
        SubscribeLocalEvent<WeakToHolyComponent, InteractUsingEvent>(AfterBibleUse);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnDamageModify);

    }

    private void AfterBibleUse(Entity<WeakToHolyComponent> ent, ref InteractUsingEvent args)
    {
        _goobBible.TryDoSmite(args.Used, args.User, args.Target);
    }

    #region Holy Damage Dealing

    private void OnDamageModify(Entity<DamageableComponent> ent, ref DamageModifyEvent args)
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
        if (uid.Comp.AlwaysTakeHoly || TryComp<HereticComponent>(uid, out var heretic) && heretic.Ascended)
        {
            args.ShouldTakeHoly = true;
            return;
        }

        // If any item in hand or in inventory has Unholy item, shouldtakeholy is true.
        if (_inventorySystem.GetHandOrInventoryEntities(args.Target, SlotFlags.WITHOUT_POCKET)
            .Any(HasComp<UnholyItemComponent>))
            args.ShouldTakeHoly = true;
    }

    #endregion

    #region Holy Healing

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

        // Healing while standing on runes.
        var query = EntityQueryEnumerator<WeakToHolyComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Rune healing
            if (comp.NextSpecialHealTick <= _timing.CurTime && comp.IsColliding)
            {
                _damageableSystem.TryChangeDamage(uid, comp.HealAmount);
                comp.NextSpecialHealTick = _timing.CurTime + comp.HealTickDelay;
            }

            // Passive healing
            if (comp.NextPassiveHealTick <= _timing.CurTime)
            {
                _damageableSystem.TryChangeDamage(uid, comp.PassiveAmount);
                comp.NextPassiveHealTick = _timing.CurTime + comp.HealTickDelay;
            }
        }

    }

    #endregion
}
