// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.Components;
using Content.Shared.SubFloor;
using Robust.Shared.Map.Components;

namespace Content.Server.SubFloor;

public sealed class SubFloorHideSystem : SharedSubFloorHideSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubFloorHideComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        SubscribeLocalEvent<SubFloorHideComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
    }

    private void OnAnchorAttempt(EntityUid uid, SubFloorHideComponent component, AnchorAttemptEvent args)
    {
        // No teleporting entities through floor tiles when anchoring them.
        var xform = Transform(uid);

        if (TryComp<MapGridComponent>(xform.GridUid, out var grid)
            && HasFloorCover(xform.GridUid.Value, grid, Map.TileIndicesFor(xform.GridUid.Value, grid, xform.Coordinates)))
        {
            args.Cancel();
        }
    }

    private void OnUnanchorAttempt(EntityUid uid, SubFloorHideComponent component, UnanchorAttemptEvent args)
    {
        // No un-anchoring things under the floor. Only required for something like vents, which are still interactable
        // despite being partially under the floor.
        if (component.IsUnderCover)
            args.Cancel();
    }
}