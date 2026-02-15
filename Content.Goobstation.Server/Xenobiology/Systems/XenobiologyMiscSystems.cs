// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.EntityEffects;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Xenobiology.Systems;

// any other bs needed serverside
public class XenobiologyMiscSystems : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ReactiveComponent, ExtinguishNearby>(OnExtinguish);
        SubscribeLocalEvent<ReactiveComponent, OxygenateNearby>(OnOxygenate);
        SubscribeLocalEvent<ReactiveComponent, IgniteNearbyEffect>(OnIgniteNearby);
        SubscribeLocalEvent<ReactiveComponent, DoSmokeEntityEffect>(OnSmoke);
    }

    public void OnExtinguish(EntityUid uid, ReactiveComponent component, ref ExtinguishNearby args)
    {

        var lookupSys = EntityManager.System<EntityLookupSystem>();
        var flamSys = EntityManager.System<FlammableSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(uid, args.Range))
        {
            if (EntityManager.TryGetComponent(entity, out FlammableComponent? flammable))
                flamSys.Extinguish(entity, flammable);
        }
    }

    public void OnOxygenate(EntityUid uid, ReactiveComponent component, ref OxygenateNearby args)
    {
        var lookupSys = EntityManager.System<EntityLookupSystem>();
        var respSys = EntityManager.System<RespiratorSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(uid, args.Range))
        {
            if (EntityManager.TryGetComponent(entity, out RespiratorComponent? resp))
                respSys.UpdateSaturation(entity, args.Factor, resp);
        }
    }

    public void OnIgniteNearby(EntityUid uid, ReactiveComponent component, ref IgniteNearbyEffect args)
    {
        var lookupSys = EntityManager.System<EntityLookupSystem>();
        var flamSys = EntityManager.System<FlammableSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(uid, args.Radius))
        {
            if (EntityManager.TryGetComponent(entity, out FlammableComponent? flammable))
                flamSys.AdjustFireStacks(entity, args.FireStacks, flammable, true);
        }
    }

    public void OnSmoke(EntityUid uid, ReactiveComponent component, ref DoSmokeEntityEffect args)
    {

        var mapMan = IoCManager.Resolve<IMapManager>();
        var transformSys = EntityManager.System<SharedTransformSystem>();
        var spreaderSys = EntityManager.System<SpreaderSystem>();
        var smokeSys = EntityManager.System<SmokeSystem>();

        if (!EntityManager.TryGetComponent(uid, out TransformComponent? xform))
            return;

        var mapCoords = transformSys.GetMapCoordinates(uid, xform);


        if (!mapMan.TryFindGridAt(mapCoords, out _, out var grid)
            || !grid.TryGetTileRef(xform.Coordinates, out var tileRef)
            || tileRef.Tile.IsEmpty)
            return;

        if (spreaderSys.RequiresFloorToSpread(args.SmokePrototype.ToString()) && tileRef.Tile.IsEmpty)
            return;

        var coords = grid.MapToGrid(mapCoords);
        var ent = EntityManager.SpawnAtPosition(args.SmokePrototype, coords.SnapToGrid());
        if (!EntityManager.TryGetComponent<SmokeComponent>(ent, out var smoke))
        {
            EntityManager.QueueDeleteEntity(ent);
            return;
        }

        smokeSys.StartSmoke(ent, args.Solution, args.Duration, args.SpreadAmount, smoke);
    }

}
