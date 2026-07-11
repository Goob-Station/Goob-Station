// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Blocking;
using Content.Shared.BloodCult.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

namespace Content.Shared.BloodCult;

public sealed class SharedBloodCultItemSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultItemComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<BloodCultItemComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BloodCultItemComponent, BeforeThrowEvent>(OnBeforeThrow);
        SubscribeLocalEvent<BloodCultItemComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<BloodCultItemComponent, AttemptMeleeEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<BloodCultItemComponent, ToggleActionEvent>(OnToggleAction,
            before: [typeof(BlockingSystem)]);
    }

    private void OnActivate(Entity<BloodCultItemComponent> item, ref ActivateInWorldEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.User))
            return;

        args.Handled = true;
        Reject(item, args.User, Loc.GetString("blood-cult-item-reject"));
    }

    private void OnUseInHand(Entity<BloodCultItemComponent> item, ref UseInHandEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.User) ||
            TryComp<EmbeddableProjectileComponent>(item, out var embeddable) && embeddable.EmbeddedIntoUid != null)
            return;

        args.Handled = true;
        Reject(item, args.User, Loc.GetString("blood-cult-item-reject"));
    }

    private void OnBeforeThrow(Entity<BloodCultItemComponent> item, ref BeforeThrowEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.PlayerUid))
            return;

        args.Cancelled = true;
        Reject(item, args.PlayerUid, Loc.GetString("blood-cult-item-throw-reject"), false);
    }

    private void OnEquipAttempt(Entity<BloodCultItemComponent> item, ref BeingEquippedAttemptEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.Equipee))
            return;

        args.Cancel();
        Reject(item, args.Equipee, Loc.GetString("blood-cult-item-equip-reject"));
    }

    private void OnMeleeAttempt(Entity<BloodCultItemComponent> item, ref AttemptMeleeEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.User))
            return;

        args.Cancelled = true;
        Reject(item, args.User, Loc.GetString("blood-cult-item-attack-reject"));
    }

    private void OnToggleAction(Entity<BloodCultItemComponent> item, ref ToggleActionEvent args)
    {
        if (_whitelist.IsWhitelistPass(item.Comp.Whitelist, args.Performer))
            return;

        args.Handled = true;
        Reject(item, args.Performer, Loc.GetString("blood-cult-item-block-reject"));
    }

    private void Reject(Entity<BloodCultItemComponent> item, EntityUid user, string message, bool predicted = true)
    {
        if (predicted)
            _popup.PopupPredicted(message, item, user);
        else
            _popup.PopupEntity(message, item, user);

        _stun.TryKnockdown(user, item.Comp.KnockdownDuration, true);
        _hands.TryDrop(user, item);
    }
}
