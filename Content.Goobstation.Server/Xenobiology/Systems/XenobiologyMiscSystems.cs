// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.EntityEffects;
using Content.Goobstation.Shared.EntityEffects;
using Content.Shared.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Xenobiology.Systems;

// any other bs needed serverside
public sealed class XenobiologyMiscSystems : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ReactiveComponent, ExtinguishNearby>(OnExtinguish);
        SubscribeLocalEvent<ReactiveComponent, OxygenateNearby>(OnOxygenate);
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
}
