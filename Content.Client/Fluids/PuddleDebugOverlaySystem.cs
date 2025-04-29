// SPDX-FileCopyrightText: 2022 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Fluids;
using Robust.Client.Graphics;

namespace Content.Client.Fluids;

public sealed class PuddleDebugOverlaySystem : SharedPuddleDebugOverlaySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    public readonly Dictionary<EntityUid, PuddleOverlayDebugMessage> TileData = new();
    private PuddleOverlay? _overlay;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PuddleOverlayDisableMessage>(DisableOverlay);
        SubscribeNetworkEvent<PuddleOverlayDebugMessage>(RenderDebugData);
    }

    private void RenderDebugData(PuddleOverlayDebugMessage message)
    {
        TileData[GetEntity(message.GridUid)] = message;
        if (_overlay != null)
            return;

        _overlay = new PuddleOverlay();
        _overlayManager.AddOverlay(_overlay);
    }

    private void DisableOverlay(PuddleOverlayDisableMessage message)
    {
        TileData.Clear();
        if (_overlay == null)
            return;

        _overlayManager.RemoveOverlay(_overlay);
        _overlay = null;
    }

    public PuddleDebugOverlayData[] GetData(EntityUid mapGridGridEntityId)
    {
        return TileData[mapGridGridEntityId].OverlayData;
    }
}