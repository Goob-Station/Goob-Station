// SPDX-FileCopyrightText: 2025 GoobBot
// SPDX-FileCopyrightText: 2025 Ilya246
// SPDX-FileCopyrightText: 2025 deltanedas
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory;
using Content.Server.Construction.Components;
using Content.Shared.Timing;

namespace Content.Goobstation.Server.Factory;

public sealed class InteractorSystem : SharedInteractorSystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    private EntityQuery<ConstructionComponent> _constructionQuery;
    private EntityQuery<UseDelayComponent> _useDelayQuery;

    public override void Initialize()
    {
        base.Initialize();

        _constructionQuery = GetEntityQuery<ConstructionComponent>();
        _useDelayQuery = GetEntityQuery<UseDelayComponent>();

        SubscribeLocalEvent<InteractorComponent, MachineStartedEvent>(OnStarted);
    }

    private void OnStarted(Entity<InteractorComponent> ent, ref MachineStartedEvent args)
    {
        // don't let it get spammed every tick to avoid lag machines
        if (_useDelayQuery.TryComp(ent, out var delay) && !_useDelay.TryResetDelay((ent, delay), true))
            return;

        // another doafter is already running
        if (HasDoAfter(ent))
        {
            Machine.Failed(ent.Owner);
            return;
        }

        // nothing there
        if (FindTarget(ent) is not {} target)
        {
            Machine.Failed(ent.Owner);
            return;
        }

        _constructionQuery.TryComp(target, out var construction);
        var originalCount = construction?.InteractionQueue.Count ?? 0;
        if (!InteractWith(ent, target))
        {
            // have to remove it since user's filter was bad due to unhandled interaction
            Machine.Failed(ent.Owner);
            return;
        }

        // construction supercode queues it instead of starting a doafter now, assume that queuing means it has started
        var newCount = construction?.InteractionQueue?.Count ?? 0;
        if (newCount > originalCount
            || HasDoAfter(ent))
        {
            Machine.Started(ent.Owner);
            UpdateAppearance(ent, InteractorState.Active);
        }
        else
        {
            // no doafter, complete it immediately
            Machine.Completed(ent.Owner);
            UpdateAppearance(ent);
        }
    }
}
