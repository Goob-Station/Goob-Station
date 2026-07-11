// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Server.Damage.Systems;
using Content.Server.Popups;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Throwing;
using Robust.Shared.Utility;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodCultThrowingCostSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaComponent, BeforeThrowEvent>(OnBeforeThrow, after: [typeof(PacificationSystem)]);
        SubscribeLocalEvent<BloodCultThrowingCostComponent, DamageExamineEvent>(OnDamageExamine,
            after: [typeof(DamageOtherOnHitSystem)]);
    }

    private void OnBeforeThrow(Entity<StaminaComponent> user, ref BeforeThrowEvent args)
    {
        if (args.Cancelled || !TryComp<BloodCultThrowingCostComponent>(args.ItemUid, out var throwingCost))
            return;

        if (user.Comp.CritThreshold - user.Comp.StaminaDamage <= throwingCost.StaminaCost)
        {
            args.Cancelled = true;
            _popup.PopupEntity(Loc.GetString("throw-no-stamina", ("item", args.ItemUid)), user, user);
            return;
        }

        _stamina.TakeStaminaDamage(user, throwingCost.StaminaCost, user.Comp, visual: false);
    }

    private void OnDamageExamine(Entity<BloodCultThrowingCostComponent> item, ref DamageExamineEvent args)
    {
        if (item.Comp.StaminaCost <= 0f)
            return;

        var message = FormattedMessage.FromMarkupOrThrow(
            Loc.GetString(
                "damage-stamina-cost",
                ("type", Loc.GetString("damage-throw")),
                ("cost", Math.Round(item.Comp.StaminaCost, 2).ToString("0.##"))));

        args.Message.PushNewline();
        args.Message.AddMessage(message);
    }
}
