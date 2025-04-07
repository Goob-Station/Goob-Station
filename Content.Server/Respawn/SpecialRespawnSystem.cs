// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared.Database;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Respawn;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Respawn;

public sealed class SpecialRespawnSystem : SharedSpecialRespawnSystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<SpecialRespawnSetupEvent>(OnSpecialRespawnSetup);
        SubscribeLocalEvent<SpecialRespawnComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SpecialRespawnComponent, EntityTerminatingEvent>(OnTermination);
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        //Try to compensate for restartroundnow command
        if (ev.Old == GameRunLevel.InRound && ev.New == GameRunLevel.PreRoundLobby)
            OnRoundEnd();

        switch (ev.New)
        {
            case GameRunLevel.PostRound:
                OnRoundEnd();
                break;
        }
    }

    private void OnSpecialRespawnSetup(SpecialRespawnSetupEvent ev)
    {
        if (!TryComp<SpecialRespawnComponent>(ev.Entity, out var comp))
            return;

        var xform = Transform(ev.Entity);

        if (xform.GridUid != null)
            comp.StationMap = (xform.MapUid, xform.GridUid);
    }

    private void OnRoundEnd()
    {
        var specialRespawnQuery = EntityQuery<SpecialRespawnComponent>();

        //Turn respawning off so the entity doesn't respawn during reset
        foreach (var entity in specialRespawnQuery)
        {
            entity.Respawn = false;
        }
    }

    private void OnStartup(EntityUid uid, SpecialRespawnComponent component, ComponentStartup args)
    {
        var ev = new SpecialRespawnSetupEvent(uid);
        QueueLocalEvent(ev);
    }

    private void OnTermination(EntityUid uid, SpecialRespawnComponent component, ref EntityTerminatingEvent args)
    {
        var entityMapUid = component.StationMap.Item1;
        var entityGridUid = component.StationMap.Item2;

        if (!component.Respawn || !HasComp<StationMemberComponent>(entityGridUid) || entityMapUid == null)
            return;

        if (!TryComp<MapGridComponent>(entityGridUid, out var grid) || MetaData(entityGridUid.Value).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        //Invalid prototype
        if (!_proto.HasIndex(component.Prototype))
            return;

        if (TryFindRandomTile(entityGridUid.Value, entityMapUid.Value, 10, out var coords))
            Respawn(uid, component.Prototype, coords);

        //If the above fails, spawn at the center of the grid on the station
        else
        {
            var xform = Transform(entityGridUid.Value);
            var pos = xform.Coordinates;
            var mapPos = _transform.GetMapCoordinates(entityGridUid.Value, xform: xform);
            var circle = new Circle(mapPos.Position, 2);

            var found = false;

            foreach (var tile in _map.GetTilesIntersecting(entityGridUid.Value, grid, circle))
            {
                if (tile.IsSpace(_tileDefinitionManager)
                    || _turf.IsTileBlocked(tile, CollisionGroup.MobMask)
                    || !_atmosphere.IsTileMixtureProbablySafe(entityGridUid, entityMapUid.Value,
                        grid.TileIndicesFor(mapPos)))
                {
                    continue;
                }

                pos = _turf.GetTileCenter(tile);
                found = true;

                if (found)
                    break;
            }

            Respawn(uid, component.Prototype, pos);
        }
    }

    /// <summary>
    /// Respawn the entity and log it.
    /// </summary>
    /// <param name="oldEntity">The entity being deleted</param>
    /// <param name="prototype">The prototype being spawned</param>
    /// <param name="coords">The place where it will be spawned</param>
    private void Respawn(EntityUid oldEntity, string prototype, EntityCoordinates coords)
    {
        var entity = Spawn(prototype, coords);
        _adminLog.Add(LogType.Respawn, LogImpact.High, $"{ToPrettyString(oldEntity)} was deleted and was respawned at {coords.ToMap(EntityManager, _transform)} as {ToPrettyString(entity)}");
        _chat.SendAdminAlert($"{MetaData(oldEntity).EntityName} was deleted and was respawned as {ToPrettyString(entity)}");
    }

    /// <summary>
    /// Try to find a random safe tile on the supplied grid
    /// </summary>
    /// <param name="targetGrid">The grid that you're looking for a safe tile on</param>
    /// <param name="targetMap">The map that you're looking for a safe tile on</param>
    /// <param name="maxAttempts">The maximum amount of attempts it should try before it gives up</param>
    /// <param name="targetCoords">If successful, the coordinates of the safe tile</param>
    /// <param name="checkTileMixture">Whether to skip tiles with unsafe air mixture</param>
    /// <returns></returns>
    public bool TryFindRandomTile(EntityUid targetGrid, EntityUid targetMap, int maxAttempts, out EntityCoordinates targetCoords, bool checkTileMixture = true) // Goob edit
    {
        targetCoords = EntityCoordinates.Invalid;

        if (!TryComp<MapGridComponent>(targetGrid, out var grid))
            return false;

        var xform = Transform(targetGrid);

        if (!grid.TryGetTileRef(xform.Coordinates, out var tileRef))
            return false;

        var tile = tileRef.GridIndices;

        var found = false;
        var (gridPos, _, gridMatrix) = _transform.GetWorldPositionRotationMatrix(xform);
        var gridBounds = gridMatrix.TransformBox(grid.LocalAABB);

        //Obviously don't put anything ridiculous in here
        for (var i = 0; i < maxAttempts; i++)
        {
            var randomX = _random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = _random.Next((int) gridBounds.Bottom, (int) gridBounds.Top);

            tile = new Vector2i(randomX - (int) gridPos.X, randomY - (int) gridPos.Y);
            var mapPos = grid.GridTileToWorldPos(tile);
            var mapTarget = grid.WorldToTile(mapPos);
            var circle = new Circle(mapPos, 2);

            foreach (var newTileRef in _map.GetTilesIntersecting(targetGrid, grid, circle))
            {
                if (newTileRef.IsSpace(_tileDefinitionManager) || _turf.IsTileBlocked(newTileRef, CollisionGroup.MobMask) || checkTileMixture && !_atmosphere.IsTileMixtureProbablySafe(targetGrid, targetMap, mapTarget)) // Goob edit
                    continue;

                found = true;
                targetCoords = grid.GridTileToLocal(tile);
                break;
            }

            //Found a safe tile, no need to continue
            if (found)
                break;
        }

        if (!found)
            return false;

        return true;
    }
}