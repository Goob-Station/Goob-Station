using System.Linq;
using Content.Shared._Goobstation.DumpContainerOnUse;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Pinpointer;
using Content.Shared.Roles;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Containers;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This handles contracts for syndicate contractors.
/// </summary>
public sealed class SharedContractorSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntitySystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // Init
        SubscribeLocalEvent<ContractorComponent, MapInitEvent>(OnContractorMapInit);
        SubscribeLocalEvent<ContractorMarkerComponent, MapInitEvent>(OnMarkerMapInit);

        // Events
        SubscribeLocalEvent<ContractorUplinkComponent, ContractorUiMessage>(OnUiButtonPressed);
        SubscribeLocalEvent<ContractorUplinkComponent, GotEquippedHandEvent>(OnUplinkEquipped);
        SubscribeLocalEvent<ContractorUplinkComponent, ExtractionDoAfterEvent>(OnExtractionDoAfter);
        SubscribeLocalEvent<ContractorPortalComponent, PreventCollideEvent>(OnPortalCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var uplinkQuery = EntityQueryEnumerator<ContractorUplinkComponent>();
        while (uplinkQuery.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime < comp.PortalSpawnTimer || comp.PortalSpawnTimer == TimeSpan.Zero)
                continue;

            CreatePortalAndLink(comp, uid);
        }

        var prisonerQuery = EntityQueryEnumerator<ContractorPrisonerComponent>();
        while (prisonerQuery.MoveNext(out var uid, out var prisonerComponent))
        {
            if (_gameTiming.CurTime < prisonerComponent.TimeLeft || prisonerComponent.TimeLeft == TimeSpan.Zero)
                continue;

            SpawnReturnPortal(uid, prisonerComponent);
        }

        var contractorPortalQuery = EntityQueryEnumerator<ContractorPortalComponent>();
        while (contractorPortalQuery.MoveNext(out var uid, out var contractorPortalComponent))
        {
            if (contractorPortalComponent.Used)
                QueueDel(uid);
        }
    }

    private void SpawnReturnPortal(EntityUid uid, ContractorPrisonerComponent prisonerComponent)
    {
        var facingDirection = Transform(uid).LocalRotation.GetCardinalDir();

        // Calculate the tile in front of the entity
        var offset = facingDirection.ToVec();
        var spawnCoordinates = uid.ToCoordinates().Offset(offset);

        // Spawn the return portal
        var returnPortal = Spawn("PortalBlue", spawnCoordinates);
        prisonerComponent.TimeLeft = TimeSpan.Zero;

        if (!TryComp<PortalComponent>(returnPortal, out var portalComp))
            return;

        portalComp.RandomTeleport = false;
        portalComp.CanTeleportToOtherMaps = true;

        var warpMarker = Spawn("ContractorMarker", prisonerComponent.ReturnCoordinates);
        _linkedEntitySystem.TryLink(returnPortal, warpMarker);
    }

    private void CreatePortalAndLink(ContractorUplinkComponent comp, EntityUid uid)
    {
        var extractPortal = Spawn("PortalRed", GetEntity(comp.FlareUid).ToCoordinates());

        if (!TryComp<PortalComponent>(extractPortal, out var portalComp))
            return;

        portalComp.CanTeleportToOtherMaps = true;
        portalComp.RandomTeleport = false; // prevents fucky shit
        comp.PortalSpawnTimer = TimeSpan.Zero;

        var contractorPortalComponent = EnsureComp<ContractorPortalComponent>(extractPortal);
        contractorPortalComponent.LinkedUplink = GetNetEntity(uid);

        QueueDel(GetEntity(comp.FlareUid));
        comp.FlareUid = NetEntity.Invalid;

        CheckAndSpawnNukeOpsMap();
        var warpMarker = SelectRandomWarp();
        if (warpMarker != null)
            _linkedEntitySystem.TryLink(extractPortal, warpMarker.Value);
    }

    private EntityUid? SelectRandomWarp()
    {
        var warpList = new List<EntityUid>();
        var warpEnumerator = EntityQueryEnumerator<ContractorWarpMarkerComponent>();

        while (warpEnumerator.MoveNext(out var warpUid, out _))
        {
            warpList.Add(warpUid);
        }

        if (warpList.Count > 0)
            return _random.Pick(warpList);

        Log.Warning("No warp markers found. Returning a null value");
        return null;
        // Select a random warp from the list

    }

    private void CheckAndSpawnNukeOpsMap()
    {
        var nukeOpsMap = false;


        foreach (var map in _mapSystem.GetAllMapIds())
        {
            if(!_mapSystem.TryGetMap(map, out var mapUid))
                continue;

            if (!TryGetEntityData(GetNetEntity(mapUid.Value), out _, out var metaDataComponent))
                continue;

            Log.Info(metaDataComponent.EntityName);

            if (metaDataComponent.EntityName != "Syndicate Outpost")
                continue;

            nukeOpsMap = true;
            break;
        }

        if (nukeOpsMap)
            return;

        if(_mapLoader.TryLoadMap(new ResPath("/Maps/_Goobstation/Nonstations/nukieplanet.yml"), out var nukiemap, out _, new DeserializationOptions { InitializeMaps = true }))
            _mapSystem.SetPaused(nukiemap.Value.Comp.MapId, false); // i think this is how i access the shit on da map lol
    }

    // prevent portal from colliding with the contractor or a non targetted entity
    private void OnPortalCollide(Entity<ContractorPortalComponent> ent, ref PreventCollideEvent args)
    {
        if(!TryComp<ContractorUplinkComponent>(GetEntity(ent.Comp.LinkedUplink), out var contractorUplinkComponent))
            return;

        var contractor = GetEntity(contractorUplinkComponent.User);

        if(!TryComp<ContractorComponent>(contractor, out var contractorComponent))
            return;

        if (args.OtherEntity == contractor)
            args.Cancelled = true;

        if (args.OtherEntity == GetEntity(contractorComponent.CurrentTarget))
            args.Cancelled = false;

        StartPrisonTimer(args.OtherEntity, args.OurEntity);
        ent.Comp.Used = true;
    }

    private void StartPrisonTimer(EntityUid target, EntityUid portal)
    {
        var prisonerComp = EnsureComp<ContractorPrisonerComponent>(target);
        prisonerComp.TimeLeft = _gameTiming.CurTime + prisonerComp.PrisonerTime;
        prisonerComp.ReturnCoordinates = Transform(portal).Coordinates;
        prisonerComp.Gear = InitializePrisonerContainer(target);
    }

    private EntityUid InitializePrisonerContainer(EntityUid target)
    {
        var containerEntity = Spawn("ContractorPrisonerBox", MapCoordinates.Nullspace);
        var dumpContainerOnUseComponent = EnsureComp<DumpContainerOnUseComponent>(containerEntity);
        var container = _containerSystem.EnsureContainer<Container>(containerEntity, dumpContainerOnUseComponent.ContainerId);

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(target))
        {
            _containerSystem.Insert(item, container, force: true);
        }

        return containerEntity;
    }

    private void OnExtractionDoAfter(Entity<ContractorUplinkComponent> ent, ref ExtractionDoAfterEvent args)
    {
        if (args.Handled || !_net.IsServer)
            return;

        if (args.Cancelled)
            return;

        var flare = SpawnAtPosition(ent.Comp.Flare, args.User.ToCoordinates());
        _transform.SetLocalRotation(flare, Angle.Zero);
        ent.Comp.FlareUid = GetNetEntity(flare);
        ent.Comp.PortalSpawnTimer = _gameTiming.CurTime + ent.Comp.PortalSpawnTime;
        args.Handled = true;
    }


    private void OnUplinkEquipped(Entity<ContractorUplinkComponent> ent, ref GotEquippedHandEvent args)
    {
        if(!_net.IsServer || ent.Comp.User != NetEntity.Invalid)
            return;

        EnsureComp<ContractorComponent>(args.User);
        ent.Comp.User = GetNetEntity(args.User);
    }


    private void OnMarkerMapInit(Entity<ContractorMarkerComponent> ent, ref MapInitEvent args)
    {
        var entitiesIntersecting = _lookupSystem.GetEntitiesIntersecting(ent.Owner.ToCoordinates());
        foreach (var entity in entitiesIntersecting)
        {
            if (!TryComp<NavMapBeaconComponent>(entity, out var navMapBeaconComponent))
                continue;

            if (navMapBeaconComponent.DefaultText == null)
                continue;

            ent.Comp.Name = navMapBeaconComponent.DefaultText;
            ent.Comp.TcReward = 4; // de-hardcode lol
            Dirty(ent, ent.Comp);
            break;
        }
    }

    private void OnContractorMapInit(Entity<ContractorComponent> ent, ref MapInitEvent args)
    {
        SetupContracts(ent);
        UpdateUi(ent);
    }

    private void UpdateUi(EntityUid uid)
    {
        if (!_handsSystem.TryGetActiveItem(uid, out var item) || !_userInterfaceSystem.HasUi(item.Value, ContractorUplinkUiKey.Key) || !TryComp<ContractorComponent>(uid, out var contractorComponent))
            return;

        var state = new ContractorUplinkBoundUserInterfaceState(contractorComponent.Tc,
            contractorComponent.Contracts,
            contractorComponent.Rep,
            contractorComponent.CurrentTarget,
            contractorComponent.CurrentExtractionPoint);

        _userInterfaceSystem.SetUiState(item.Value, ContractorUplinkUiKey.Key, state);
    }

    private void OnUiButtonPressed(Entity<ContractorUplinkComponent> ent, ref ContractorUiMessage msg) // extract the switch
    {
        if (!_net.IsServer)
            return;

        var user = msg.Actor;
        if (!Exists(user))
            return;

        if(!TryComp<ContractorComponent>(user, out var contractorComponent))
            return;

        var loc = msg.Location;

        switch (msg.Button)
        {
            case UiMessage.SelectTarget:
                if(contractorComponent.Contracts.ContainsKey(msg.Target))
                    contractorComponent.CurrentTarget = msg.Target;
                if(contractorComponent.Contracts.Values.Any(subList => subList.Contains(loc))) // this is evil
                    contractorComponent.CurrentExtractionPoint = msg.Location;
                if (!TryComp<ContractorMarkerComponent>(GetEntity(msg.Location), out var contractorMarkerComponent))
                    return;
                contractorComponent.TcReward = contractorMarkerComponent.TcReward;
                break;
            case UiMessage.TryExtraction:
                // light flare do after
                var lookup = _lookupSystem.GetEntitiesInRange<ContractorMarkerComponent>(user.ToCoordinates(), 8.0f);
                if (!lookup.Select(entity => entity.Owner)
                    .Contains(GetEntity(contractorComponent.CurrentExtractionPoint)))
                    break;

                var doAfterEventArgs = new DoAfterArgs(EntityManager,
                    user,
                    3.0f,
                    new ExtractionDoAfterEvent(),
                    ent.Owner,
                    used: ent.Owner)
                {
                    BreakOnMove = true,
                    BreakOnDamage = true,
                };

                if (!_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
                    return;

                break;
            case UiMessage.Refresh:
                Log.Info("Refresh requested");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateUi(msg.Actor);
    }

    private void SetupContracts(Entity<ContractorComponent> ent)
    {
        if (!_net.IsServer) // I really feel like this is the really lazy way of doing this instead of moving it to server but... alas we are here.
            return;
        // get ready for the craziest linq maxing
        var possibleContracts = _mindSystem.GetAliveHumans();

        foreach (var humanoid in possibleContracts)
        foreach (var mindRole in humanoid.Comp.MindRoles) // cleanse the list of yourself
            if (!TryComp<MindRoleComponent>(mindRole, out var mindRoleComp) || mindRoleComp.Antag || //allow antags
                humanoid.Comp.OwnedEntity == ent.Owner || humanoid.Comp.OwnedEntity == null)
                possibleContracts.Remove(humanoid);

        if (possibleContracts.Count == 0)
        {
            Log.Debug("Not enough alive humanoids to generate a contract"); // add a refresh timer or sum IDK need a brain
            return;
        }

        var targets = possibleContracts.OrderBy(_ => _random.Next())
            .Take(5)
            .Select(entity => GetNetEntity(entity.Comp.OwnedEntity))
            .ToList();

        var query = EntityQueryEnumerator<ContractorMarkerComponent>();
        var markerList = new List<NetEntity>();

        while (query.MoveNext(out var uid, out _))
        {
            markerList.Add(GetNetEntity(uid));
        }

        foreach (var target in targets)
        {
            if (target == null)
                return;

            ent.Comp.Contracts.Add(target.Value, markerList.OrderBy(_ => _random.Next()).Take(3).ToList());
            Dirty(ent, ent.Comp); // dirty since this is the server
        }
    }
}
