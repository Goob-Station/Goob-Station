// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.DeployableGrid;

public abstract class SharedDeployableGridSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeployableGridComponent, UseInHandEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, DeployableGridComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.DeployTime, new DeployableGridDoAfterEvent(), uid, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!CheckCanDeploy((uid, component)))
        {
            args.Handled = true;
            return;
        }

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        args.Handled = true;
    }

    protected bool CheckCanDeploy(Entity<DeployableGridComponent> ent)
    {
        var xform = Transform(ent);
        var comp = ent.Comp;

        // Works only on planets!
        if (xform.GridUid == null || xform.MapUid == null || xform.GridUid != xform.MapUid || !TryComp<MapGridComponent>(xform.GridUid.Value, out _))
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-planet"), xform.Coordinates);
            return false;
        }

        var worldPos = _transform.GetMapCoordinates(ent, xform);

        // Make sure that surrounding area does not have any entities with physics
        var box = Box2.CenteredAround(worldPos.Position.Rounded(), comp.BoxSize);

        #region DOWNSTREAM-TPirates: bluespace shelter capsules fix
        // Doesn't work near other grids (5×5 / 7×7 deploy box)
        var nearbyGrids = new List<Entity<MapGridComponent>>();
        _mapManager.FindGridsIntersecting(worldPos.MapId, box, ref nearbyGrids, includeMap: false);
        nearbyGrids.RemoveAll(e => e.Owner == xform.GridUid.Value);
        if (nearbyGrids.Count > 0)
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-near-grid"), xform.Coordinates);
            return false;
        }

        if (GetBlockingEntities(xform.GridUid.Value, box).Any())
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-space"), xform.Coordinates);
            return false;
        }
        #endregion

        return true;
    }

    #region DOWNSTREAM-TPirates: bluespace shelter capsules fix
    private IEnumerable<EntityUid> GetBlockingEntities(EntityUid gridUid, Box2 worldBox)
    {
        foreach (var uid in _lookup.GetEntitiesIntersecting(gridUid, worldBox, LookupFlags.Static | LookupFlags.Sensors))
        {
            if (TryComp<PhysicsComponent>(uid, out var phys) &&
                phys.BodyType == BodyType.Static &&
                phys.Hard &&
                (phys.CollisionLayer & (int) CollisionGroup.Impassable) != 0)
            {
                yield return uid;
            }
            else if (IsHazardousStepTrigger(uid))
            {
                yield return uid;
            }
        }
    }

    private bool IsHazardousStepTrigger(EntityUid uid)
    {
        ProtoId<StepTriggerTypePrototype>[] hazardousStepTriggerTypeIds = [new("Lava"), new("Chasm")];
        if (!TryComp<StepTriggerComponent>(uid, out var step) || step.TriggerGroups?.Types == null)
            return false;
        var types = step.TriggerGroups.Types;
        return hazardousStepTriggerTypeIds.Any(t => types.Contains(t));
    }
    #endregion
}
