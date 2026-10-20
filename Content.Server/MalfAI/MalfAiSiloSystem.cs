// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Power.Components;
using Content.Server.Silicons.StationAi;
using Content.Shared.Database;
using Content.Shared.Lathe;
using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Materials.OreSilo;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles deployment and automatic linking of the Malfunctioning Silo.
/// Connects all factory machines and ore silo clients on the grid so they use the silo's materials.
/// </summary>
public sealed class MalfAiSiloSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedOreSiloSystem _oreSilo = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiPlaceSiloActionEvent>(OnPlaceSiloAction);
        SubscribeLocalEvent<MalfAiSiloComponent, ComponentStartup>(OnSiloStartup);
        SubscribeLocalEvent<OreSiloClientComponent, ComponentStartup>(OnClientStartup);
        SubscribeLocalEvent<LatheComponent, ComponentStartup>(OnLatheStartup);
        SubscribeLocalEvent<MalfFactoryEntityComponent, ComponentStartup>(OnFactoryStartup);
    }

    private void OnPlaceSiloAction(MalfAiPlaceSiloActionEvent args)
    {
        if (args.Handled)
            return;

        var performer = args.Performer;
        if (!TryGetMalfAi(performer, out var malfAi))
            return;

        var popupTarget = GetAiEyeForPopup(malfAi) ?? performer;

        var targetCoords = args.Target;
        if (!targetCoords.IsValid(EntityManager))
        {
            _popup.PopupEntity(Loc.GetString("malfai-silo-invalid-location"), popupTarget, performer, PopupType.SmallCaution);
            return;
        }

        var gridUid = _transform.GetGrid(targetCoords);
        if (gridUid == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-silo-must-be-on-grid"), popupTarget, performer, PopupType.SmallCaution);
            return;
        }

        if (!IsTileFree(targetCoords))
        {
            _popup.PopupEntity(Loc.GetString("malfai-silo-location-blocked"), popupTarget, performer, PopupType.SmallCaution);
            return;
        }

        var spawned = Spawn("MachineMaterialSiloMalfunctioning", targetCoords);
        if (TryComp<TransformComponent>(spawned, out var xform))
        {
            _transform.AnchorEntity(spawned, xform);
        }

        if (TryComp<ApcPowerReceiverComponent>(spawned, out var powerReceiver))
        {
            powerReceiver.NeedsPower = false;
            powerReceiver.Powered = true;
            Dirty(spawned, powerReceiver);
        }

        if (TryComp<MalfAiSiloComponent>(spawned, out var siloComp))
        {
            siloComp.Controller = malfAi;
            Dirty(spawned, siloComp);
        }

        if (TryComp<OreSiloComponent>(spawned, out var oreSilo))
        {
            LinkAllFactoryEntitiesToSilo((spawned, oreSilo));
        }

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg"), spawned);
        _popup.PopupEntity(Loc.GetString("malfai-silo-placed-success"), popupTarget, performer, PopupType.Medium);
        _adminLogger.Add(LogType.Action, LogImpact.High, $"Malf AI {ToPrettyString(malfAi)} placed Malfunctioning Silo {ToPrettyString(spawned)} at {targetCoords}");

        args.Handled = true;
    }

    private void OnSiloStartup(Entity<MalfAiSiloComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<ApcPowerReceiverComponent>(ent, out var powerReceiver))
        {
            powerReceiver.NeedsPower = false;
            powerReceiver.Powered = true;
            Dirty(ent.Owner, powerReceiver);
        }

        if (TryComp<OreSiloComponent>(ent, out var oreSilo))
        {
            LinkAllFactoryEntitiesToSilo((ent.Owner, oreSilo));
        }
    }

    private void OnClientStartup(Entity<OreSiloClientComponent> ent, ref ComponentStartup args)
    {
        var clientGrid = _transform.GetGrid(ent.Owner);
        if (clientGrid == null)
            return;

        var siloQuery = EntityQueryEnumerator<MalfAiSiloComponent, OreSiloComponent, TransformComponent>();
        while (siloQuery.MoveNext(out var siloUid, out _, out var oreSilo, out var xform))
        {
            if (xform.GridUid == clientGrid)
            {
                _oreSilo.LinkClient((siloUid, oreSilo), ent);
                break;
            }
        }
    }

    private void OnLatheStartup(Entity<LatheComponent> ent, ref ComponentStartup args)
    {
        var latheGrid = _transform.GetGrid(ent.Owner);
        if (latheGrid == null)
            return;

        var siloQuery = EntityQueryEnumerator<MalfAiSiloComponent, OreSiloComponent, TransformComponent>();
        while (siloQuery.MoveNext(out var siloUid, out _, out var oreSilo, out var xform))
        {
            if (xform.GridUid == latheGrid)
            {
                var clientComp = EnsureComp<OreSiloClientComponent>(ent.Owner);
                _oreSilo.LinkClient((siloUid, oreSilo), (ent.Owner, clientComp));
                break;
            }
        }
    }

    private void OnFactoryStartup(Entity<MalfFactoryEntityComponent> ent, ref ComponentStartup args)
    {
        var factoryGrid = _transform.GetGrid(ent.Owner);
        if (factoryGrid == null)
            return;

        var clientComp = EnsureComp<OreSiloClientComponent>(ent.Owner);
        var siloQuery = EntityQueryEnumerator<MalfAiSiloComponent, OreSiloComponent, TransformComponent>();
        while (siloQuery.MoveNext(out var siloUid, out _, out var oreSilo, out var xform))
        {
            if (xform.GridUid == factoryGrid)
            {
                _oreSilo.LinkClient((siloUid, oreSilo), (ent.Owner, clientComp));
                break;
            }
        }
    }

    /// <summary>
    /// Links all existing factory machines and ore silo clients on the silo's grid to this silo.
    /// </summary>
    public void LinkAllFactoryEntitiesToSilo(Entity<OreSiloComponent> silo)
    {
        var siloGrid = _transform.GetGrid(silo.Owner);
        if (siloGrid == null)
            return;

        // 1. All entities with OreSiloClientComponent on this grid
        var clientQuery = EntityQueryEnumerator<OreSiloClientComponent, TransformComponent>();
        while (clientQuery.MoveNext(out var uid, out var clientComp, out var xform))
        {
            if (xform.GridUid != siloGrid)
                continue;

            _oreSilo.LinkClient(silo, (uid, clientComp));
        }

        // 2. All entities with LatheComponent on this grid
        var latheQuery = EntityQueryEnumerator<LatheComponent, TransformComponent>();
        while (latheQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid != siloGrid)
                continue;

            var clientComp = EnsureComp<OreSiloClientComponent>(uid);
            _oreSilo.LinkClient(silo, (uid, clientComp));
        }

        // 3. All entities with MalfFactoryEntityComponent on this grid
        var factoryQuery = EntityQueryEnumerator<MalfFactoryEntityComponent, TransformComponent>();
        while (factoryQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid != siloGrid)
                continue;

            var clientComp = EnsureComp<OreSiloClientComponent>(uid);
            _oreSilo.LinkClient(silo, (uid, clientComp));
        }
    }

    private bool TryGetMalfAi(EntityUid actor, out EntityUid malfAi)
    {
        if (HasComp<MalfAiMarkerComponent>(actor))
        {
            malfAi = actor;
            return true;
        }

        if (TryComp<StationAiEyeComponent>(actor, out _))
        {
            var coreQuery = AllEntityQuery<StationAiCoreComponent>();
            while (coreQuery.MoveNext(out var coreUid, out var coreComp))
            {
                if (coreComp.RemoteEntity == actor)
                {
                    if (_stationAi.TryGetHeld((coreUid, coreComp), out var held) &&
                        HasComp<MalfAiMarkerComponent>(held))
                    {
                        malfAi = held.Value;
                        return true;
                    }
                }
            }
        }

        if (TryComp<StationAiCoreComponent>(actor, out var coreComp2) &&
            _stationAi.TryGetHeld((actor, coreComp2), out var heldCore) &&
            HasComp<MalfAiMarkerComponent>(heldCore))
        {
            malfAi = heldCore.Value;
            return true;
        }

        if (_stationAi.TryGetCore(actor, out var core) &&
            _stationAi.TryGetHeld(core, out var heldBrain) &&
            HasComp<MalfAiMarkerComponent>(heldBrain))
        {
            malfAi = heldBrain.Value;
            return true;
        }

        if (TryComp<MindContainerComponent>(actor, out var mindContainer) &&
            mindContainer.Mind != null)
        {
            var mindQuery = AllEntityQuery<MalfAiMarkerComponent, MindContainerComponent>();
            while (mindQuery.MoveNext(out var mUid, out _, out var mMindContainer))
            {
                if (mMindContainer.Mind == mindContainer.Mind)
                {
                    malfAi = mUid;
                    return true;
                }
            }
        }

        malfAi = default;
        return false;
    }

    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (_stationAi.TryGetCore(aiUid, out var core) && core.Comp?.RemoteEntity != null)
            return core.Comp.RemoteEntity.Value;

        return null;
    }

    private bool IsTileFree(EntityCoordinates coordinates)
    {
        var mapCoords = _transform.ToMapCoordinates(coordinates);
        var box = Box2.CenteredAround(mapCoords.Position, new System.Numerics.Vector2(0.8f, 0.8f));
        foreach (var entity in _lookup.GetEntitiesIntersecting(mapCoords.MapId, box))
        {
            if (TryComp<PhysicsComponent>(entity, out var physics) &&
                physics.CanCollide &&
                physics.Hard &&
                TryComp<TransformComponent>(entity, out var xform) &&
                xform.Anchored)
            {
                return false;
            }
        }
        return true;
    }
}
