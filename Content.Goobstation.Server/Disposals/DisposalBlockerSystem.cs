// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Disposals.Tube.Components;
using Content.Server.Disposal.Tube;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Disposals;

/// <summary>
/// This handles...
/// </summary>
public sealed class DisposalBlockerSystem : EntitySystem
{
    [Dependency] private readonly DisposalTubeSystem _disposalSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DisposalBlockerComponent, GetDisposalsConnectableDirectionsEvent>(OnGetBlockerConnectableDirections);
        SubscribeLocalEvent<DisposalBlockerComponent, GetDisposalsNextDirectionEvent>(OnGetBlockerNextDirection);
    }
    private void OnGetBlockerConnectableDirections(EntityUid uid, DisposalBlockerComponent component, ref GetDisposalsConnectableDirectionsEvent args)
    {
        _disposalSystem.OnGetTransitConnectableDirections(uid, component, ref args);
    }

    private void OnGetBlockerNextDirection(EntityUid uid, DisposalBlockerComponent component, ref GetDisposalsNextDirectionEvent args)
    {
        var ev = new GetDisposalsConnectableDirectionsEvent();
        RaiseLocalEvent(uid, ref ev);

        args.Next = ev.Connectable[0];
    }
}