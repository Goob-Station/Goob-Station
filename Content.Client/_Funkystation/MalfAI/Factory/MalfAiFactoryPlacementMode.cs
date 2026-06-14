// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI.Factory;
using Robust.Client.Placement;
using Robust.Client.Placement.Modes;
using Robust.Shared.Map;

namespace Content.Client._Funkystation.MalfAI.Factory;

/// <summary>
/// Placement mode for the MalfAI factory ghost.
/// </summary>
public sealed class MalfAiFactoryPlacementMode : SnapgridCenter
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly SharedMalfAiFactorySystem _factory;

    public MalfAiFactoryPlacementMode(PlacementManager pMan) : base(pMan)
    {
        IoCManager.InjectDependencies(this);
        _factory = _entityManager.System<SharedMalfAiFactorySystem>();
    }

    public override bool IsValidPosition(EntityCoordinates position)
    {
        return _factory.IsTileFree(position, out _);
    }
}
