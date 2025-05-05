// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory;
using Content.Server.Construction.Components;
using Content.Server.DeviceLinking.Events;

namespace Content.Goobstation.Server.Factory;

public sealed class InteractorSystem : SharedInteractorSystem
{
    private EntityQuery<ConstructionComponent> _constructionQuery;

    public override void Initialize()
    {
        base.Initialize();

        _constructionQuery = GetEntityQuery<ConstructionComponent>();

        SubscribeLocalEvent<InteractorComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<InteractorComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.StartPort)
            StartInteracting(ent);
    }

    private void StartInteracting(Entity<InteractorComponent> ent)
    {
        if (!Power.IsPowered(ent.Owner))
            return;

        // nothing there or another doafter is already running
        var count = ent.Comp.TargetEntities.Count;
        if (count == 0 || HasDoAfter(ent))
        {
            Device.InvokePort(ent, ent.Comp.FailedPort);
            return;
        }

        var i = count - 1;
        var netEnt = ent.Comp.TargetEntities[i].Item1;
        var target = GetEntity(netEnt);
        _constructionQuery.TryComp(target, out var construction);
        var originalCount = construction?.InteractionQueue?.Count ?? 0;
        if (!InteractWith(ent, target))
        {
            // have to remove it since user's filter was bad due to unhandled interaction
            RemoveTarget(ent, target);
            Device.InvokePort(ent, ent.Comp.FailedPort);
            return;
        }

        // construction supercode queues it instead of starting a doafter now, assume that queuing means it has started
        var newCount = construction?.InteractionQueue?.Count ?? 0;
        if (newCount > originalCount || HasDoAfter(ent))
        {
            Device.InvokePort(ent, ent.Comp.StartedPort);
        }
        else
        {
            // no doafter, complete it immediately
            TryRemoveTarget(ent, target);
            Device.InvokePort(ent, ent.Comp.CompletedPort);
        }
    }
}
