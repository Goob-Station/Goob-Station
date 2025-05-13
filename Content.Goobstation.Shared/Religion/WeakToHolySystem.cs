// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Timing;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Targeting; // Shitmed Change
namespace Content.Goobstation.Shared.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GoobBibleSystem _goobBible = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyItemDamage);
        SubscribeLocalEvent<WeakToHolyComponent, AfterInteractUsingEvent>(AfterBibleUse);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnDamageModify);

    }

    private void AfterBibleUse(Entity<WeakToHolyComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!TryComp<BibleComponent>(args.Used, out var bibleComp)
            || !TryComp(args.Used, out UseDelayComponent? useDelay)
            || _useDelay.IsDelayed((args.Used, useDelay))
            || !HasComp<BibleUserComponent>(args.User)
            || args.Target is not { } target)
            return;

        _goobBible.TryDoSmite(target, bibleComp, args, useDelay);
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

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(args.Target, SlotFlags.WITHOUT_POCKET))
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

            _damageableSystem.TryChangeDamage(uid, comp.HealAmount * 11f, targetPart: TargetBodyPart.All); // Shitmed Change

            comp.NextHealTick = _timing.CurTime + comp.HealTickDelay;
        }
    }

    #endregion
}
