// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.VentCrawler.Tube.Components;
using Content.Shared._Starlight.VentCrawling.Components;
using Content.Shared._Starlight.VentCrawling;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Starlight.VentCrawling;

public sealed class VentCrawableSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VentCrawlerHolderComponent, VentCrawlingExitEvent>(OnVentCrawlingExitEvent);
    }

    /// <summary>
    /// Exits the vent craws for the specified VentCrawlerHolderComponent, removing it and any contained entities from the craws.
    /// </summary>
    /// <param name="uid">The EntityUid of the VentCrawlerHolderComponent.</param>
    /// <param name="holder">The VentCrawlerHolderComponent instance.</param>
    /// <param name="holderTransform">The TransformComponent instance for the VentCrawlerHolderComponent.</param>
    private void OnVentCrawlingExitEvent(EntityUid uid, VentCrawlerHolderComponent holder, ref VentCrawlingExitEvent args)
    {
        var holderTransform = args.holderTransform;

        if (Terminating(uid))
            return;

        if (!Resolve(uid, ref holderTransform))
            return;

        if (holder.IsExitingVentCraws)
        {
            Log.Error("Tried exiting VentCraws twice. This should never happen.");
            return;
        }

        holder.IsExitingVentCraws = true;

        foreach (var entity in holder.Container.ContainedEntities.ToArray())
        {
            RemComp<BeingVentCrawlerComponent>(entity);

            var meta = MetaData(entity);
            _containerSystem.Remove(entity, holder.Container, reparent: false, force: true);

            var xform = Transform(entity);
            if (xform.ParentUid != uid)
                continue;

            _xformSystem.AttachToGridOrMap(entity, xform);

            if (TryComp<VentCrawlerComponent>(entity, out var ventCrawComp))
            {
                ventCrawComp.InTube = false;
                Dirty(entity , ventCrawComp);
            }

            if (EntityManager.TryGetComponent(entity, out PhysicsComponent? physics))
                _physicsSystem.WakeBody(entity, body: physics);
        }

        EntityManager.DeleteEntity(uid);
    }
}