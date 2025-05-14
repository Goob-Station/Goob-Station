// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Timing; // Shitmed Change
namespace Content.Goobstation.Shared.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GoobBibleSystem _goobBible = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyItemDamage);
        SubscribeLocalEvent<WeakToHolyComponent, InteractUsingEvent>(AfterBibleUse);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnHolyDamageModify);

    }

    private void AfterBibleUse(Entity<WeakToHolyComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<BibleComponent>(args.Used, out var bibleComp))
            return;

        if (!TryComp(args.Used, out UseDelayComponent? useDelay)
            || _useDelay.IsDelayed((args.Used, useDelay))
            || !HasComp<BibleUserComponent>(args.User))
            return;

        _goobBible.TryDoSmite(args.Used, args.User, args.Target, useDelay);
    }

    #region Holy Damage Dealing

    private void OnHolyDamageModify(Entity<DamageableComponent> ent, ref DamageModifyEvent args)
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

        if (!TryComp<BodyComponent>(ent, out var body))
            return;

        if (!_body.TryGetRootPart(ent, out var rootPart, body: body))
            return;

        foreach (var woundable in _wound.GetAllWoundableChildren(rootPart.Value))
        {
            if (HasComp<DamageableComponent>(woundable))
                args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifierSet);
        }
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

        // Holy damage healing.
        var query = EntityQueryEnumerator<BodyComponent, WeakToHolyComponent>();
        while (query.MoveNext(out var uid, out var body, out var weakToHoly))
        {
            if (!TryComp<DamageableComponent>(uid, out var damageable))
                return;

            if (TerminatingOrDeleted(uid)
                || !_body.TryGetRootPart(uid, out var rootPart, body: body)
                || !damageable.Damage.DamageDict.TryGetValue("Holy", out _))
                return;

            // Rune healing.
            if (weakToHoly.NextSpecialHealTick <= _timing.CurTime && weakToHoly.IsColliding)
            {
                _damageableSystem.TryChangeDamage(uid, weakToHoly.HealAmount, ignoreBlockers: true, targetPart: TargetBodyPart.All);
                weakToHoly.NextSpecialHealTick = _timing.CurTime + weakToHoly.HealTickDelay;
            }

            // Passive healing.
            _damageableSystem.TryChangeDamage(uid, weakToHoly.PassiveAmount, ignoreBlockers: true, targetPart: TargetBodyPart.All);
            weakToHoly.NextPassiveHealTick = _timing.CurTime + weakToHoly.HealTickDelay;
        }
    }

    #endregion
}
