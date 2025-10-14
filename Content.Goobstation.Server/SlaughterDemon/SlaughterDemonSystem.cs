// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Goobstation.Shared.SlaughterDemon.Systems;
using Content.Server.Administration.Systems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class SlaughterDemonSystem : SharedSlaughterDemonSystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;

    private EntityQuery<BloodstreamComponent> _bloodstreamQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _bloodstreamQuery = GetEntityQuery<BloodstreamComponent>();

        SubscribeLocalEvent<SlaughterDemonComponent, BeingGibbedEvent>(OnGib);
    }

    private void OnGib(Entity<SlaughterDemonComponent> ent, ref BeingGibbedEvent args)
    {
        if (!TryComp<SlaughterDevourComponent>(ent.Owner, out var devour)
            || devour.Container == null)
            return;

        _container.EmptyContainer(devour.Container);

        // heal them if they were in the laughter demon
        if (!ent.Comp.IsLaughter)
            return;

        foreach (var entity in ent.Comp.ConsumedMobs)
            _rejuvenate.PerformRejuvenate(entity);
    }

    protected override void RemoveBlood(EntityUid uid)
    {
        base.RemoveBlood(uid);

        if (!_bloodstreamQuery.TryComp(uid, out var comp))
            return;

        _bloodstream.SpillAllSolutions(uid, comp);
    }
}
