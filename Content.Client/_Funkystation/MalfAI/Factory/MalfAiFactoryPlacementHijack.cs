// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Client.Placement;
using Robust.Shared.Map;

namespace Content.Client._Funkystation.MalfAI.Factory;

/// <summary>
/// Intercepts the placement click and notifies the ghost system via callback instead of spawning an entity.
/// </summary>
public sealed class MalfAiFactoryPlacementHijack : PlacementHijack
{
    private readonly Action<EntityCoordinates> _onPlaced;

    public override bool CanRotate => false;

    public MalfAiFactoryPlacementHijack(Action<EntityCoordinates> onPlaced)
    {
        _onPlaced = onPlaced;
    }

    public override bool HijackPlacementRequest(EntityCoordinates coordinates)
    {
        _onPlaced(coordinates);
        Manager.Clear();
        return true;
    }
}
