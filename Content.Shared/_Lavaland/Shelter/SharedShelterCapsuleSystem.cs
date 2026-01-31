// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
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

namespace Content.Shared._Lavaland.Shelter;

public abstract class SharedShelterCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, UseInHandEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, ShelterCapsuleComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.DeployTime, new ShelterCapsuleDeployDoAfterEvent(), uid, used: uid)
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

    protected bool CheckCanDeploy(Entity<ShelterCapsuleComponent> ent)
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