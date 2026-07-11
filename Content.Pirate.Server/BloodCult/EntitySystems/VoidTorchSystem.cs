// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Server.Hands.Systems;
using Content.Server.Interaction;
using Content.Server.Popups;
using Content.Shared._White.ListViewSelector;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class VoidTorchSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private readonly Dictionary<EntityUid, PendingVoidTorchTransfer> _pendingTransfers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidTorchComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidTorchComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VoidTorchComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<VoidTorchComponent, ListViewItemSelectedMessage>(OnCultistSelected);
        SubscribeLocalEvent<VoidTorchComponent, BoundUIClosedEvent>(OnUiClosed);
    }

    private void OnStartup(Entity<VoidTorchComponent> torch, ref ComponentStartup args)
    {
        _appearance.SetData(torch, BloodCultVisuals.Active, torch.Comp.Charges > 0);
    }

    private void OnShutdown(Entity<VoidTorchComponent> torch, ref ComponentShutdown args)
    {
        _pendingTransfers.Remove(torch);
    }

    private void OnAfterInteract(Entity<VoidTorchComponent> torch, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || torch.Comp.Charges <= 0 || args.Target is not { } item ||
            !HasComp<ItemComponent>(item) ||
            (!HasComp<BloodCultistComponent>(args.User) && !HasComp<BloodCultConstructComponent>(args.User)))
            return;

        var targets = new HashSet<EntityUid>();
        var entries = new List<ListViewSelectorEntry>();
        var query = EntityQueryEnumerator<BloodCultistComponent>();

        while (query.MoveNext(out var cultist, out _))
        {
            if (cultist == args.User || !Exists(cultist))
                continue;

            targets.Add(cultist);
            var metadata = MetaData(cultist);
            entries.Add(new ListViewSelectorEntry(cultist.ToString(), metadata.EntityName, metadata.EntityDescription));
        }

        args.Handled = true;
        if (entries.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("void-torch-no-targets"), torch, args.User);
            return;
        }

        _pendingTransfers[torch] = new PendingVoidTorchTransfer(args.User, item, targets);
        _ui.SetUiState(torch.Owner, ListViewSelectorUiKey.Key, new ListViewSelectorState(entries));
        if (!_ui.TryOpenUi(torch.Owner, ListViewSelectorUiKey.Key, args.User))
            _pendingTransfers.Remove(torch);
    }

    private void OnCultistSelected(Entity<VoidTorchComponent> torch, ref ListViewItemSelectedMessage args)
    {
        if (!_pendingTransfers.Remove(torch, out var pending) ||
            pending.Actor != args.Actor ||
            !_ui.IsUiOpen(torch.Owner, ListViewSelectorUiKey.Key, args.Actor) ||
            !EntityUid.TryParse(args.SelectedItem.Id, out var target) ||
            !pending.Targets.Contains(target) ||
            torch.Comp.Charges <= 0 ||
            !Exists(pending.Item) ||
            !Exists(target) ||
            !HasComp<ItemComponent>(pending.Item) ||
            !HasComp<BloodCultistComponent>(target) ||
            !_hands.IsHolding(args.Actor, torch) ||
            !_interaction.InRangeUnobstructed(args.Actor, pending.Item))
        {
            _ui.CloseUi(torch.Owner, ListViewSelectorUiKey.Key, args.Actor);
            return;
        }

        if (_hands.IsHolding(args.Actor, pending.Item))
            _hands.TryDrop(args.Actor, pending.Item);
        else if (_container.IsEntityOrParentInContainer(pending.Item))
        {
            _ui.CloseUi(torch.Owner, ListViewSelectorUiKey.Key, args.Actor);
            return;
        }

        _transform.SetCoordinates(pending.Item, Transform(target).Coordinates);
        _hands.TryPickupAnyHand(target, pending.Item);
        _audio.PlayPvs(torch.Comp.TeleportSound, torch);

        torch.Comp.Charges--;
        if (torch.Comp.Charges == 0)
            _appearance.SetData(torch, BloodCultVisuals.Active, false);

        _ui.CloseUi(torch.Owner, ListViewSelectorUiKey.Key, args.Actor);
    }

    private void OnUiClosed(Entity<VoidTorchComponent> torch, ref BoundUIClosedEvent args)
    {
        if (Equals(args.UiKey, ListViewSelectorUiKey.Key) &&
            _pendingTransfers.TryGetValue(torch, out var pending) &&
            pending.Actor == args.Actor)
            _pendingTransfers.Remove(torch);
    }

    private readonly record struct PendingVoidTorchTransfer(
        EntityUid Actor,
        EntityUid Item,
        HashSet<EntityUid> Targets);
}
