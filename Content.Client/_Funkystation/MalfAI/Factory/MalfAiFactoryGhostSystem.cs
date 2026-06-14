// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared._Funkystation.MalfAI.Factory;
using Robust.Client.Placement;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._Funkystation.MalfAI.Factory;


public sealed class MalfAiFactoryGhostSystem : EntitySystem
{
    [Dependency] private readonly IPlacementManager _placement = default!;

    private const string FactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiRoboticsFactoryActionEvent>(OnFactoryAction);
    }

    private void OnFactoryAction(Entity<MalfAiMarkerComponent> ent, ref MalfAiRoboticsFactoryActionEvent args)
    {
        if (_placement.IsActive)
        {
            _placement.Clear();
            return;
        }

        var performer = GetNetEntity(ent.Owner);

        _placement.BeginPlacing(new PlacementInformation
        {
            IsTile = false,
            PlacementOption = nameof(MalfAiFactoryPlacementMode),
            EntityType = FactoryPrototype,
        }, new MalfAiFactoryPlacementHijack(coords =>
        {
            RaiseNetworkEvent(new MalfAiFactoryBuildNetEvent(performer, GetNetCoordinates(coords)));
        }));

        if (_placement is PlacementManager pm)
        {
            var overlayEnt = EntityManager.SpawnEntity(FactoryPrototype, MapCoordinates.Nullspace);
            EntityManager.RunMapInit(overlayEnt, MetaData(overlayEnt));
            pm.CurrentPlacementOverlayEntity = overlayEnt;
        }
    }
}
