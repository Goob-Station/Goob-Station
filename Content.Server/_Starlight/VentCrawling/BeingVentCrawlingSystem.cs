// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Starlight.VentCrawling.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Throwing;

namespace Content.Server._Starlight.VentCrawling;

public sealed class BeingVentCrawSystem : EntitySystem
{
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingVentCrawlerComponent, InhaleLocationEvent>(OnInhaleLocation);
        SubscribeLocalEvent<BeingVentCrawlerComponent, ExhaleLocationEvent>(OnExhaleLocation);
        SubscribeLocalEvent<BeingVentCrawlerComponent, AtmosExposedGetAirEvent>(OnGetAir);

        // List of all actions you cant do while in the pipe
        SubscribeLocalEvent<BeingVentCrawlerComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, ThrowAttemptEvent>(OnThrowAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, IsUnequippingAttemptEvent>(OnUnequiptAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, IsEquippingAttemptEvent>(OnEquiptAttempt);

    }

    private void OnGetAir(EntityUid uid, BeingVentCrawlerComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            args.Handled = true;
            return;
        }
    }

    private void OnInhaleLocation(EntityUid uid, BeingVentCrawlerComponent component, InhaleLocationEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            return;
        }
    }

    private void OnExhaleLocation(EntityUid uid, BeingVentCrawlerComponent component, ExhaleLocationEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            return;
        }
    }

    private void OnActionAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref ActionAttemptEvent args)
        => args.Cancelled = true;

    private void OnAttackAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref AttackAttemptEvent args)
        => args.Cancel();

    private void OnPickupAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref PickupAttemptEvent args)
        => args.Cancel();

    private void OnThrowAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref ThrowAttemptEvent args)
        => args.Cancel();

    private void OnDropAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref DropAttemptEvent args)
        => args.Cancel();

    private void OnUnequiptAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref IsUnequippingAttemptEvent args)
        => args.Cancel();

    private void OnEquiptAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref IsEquippingAttemptEvent args)
        => args.Cancel();
}