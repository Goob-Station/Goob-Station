using System.Linq;
using Content.Shared._Goobstation.Contracts.Components;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Pinpointer;
using Content.Shared.Roles;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Containers;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This handles contracts for syndicate contractors.
/// </summary>
public sealed partial class SharedContractorSystem : EntitySystem
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

        SubscribeLocalEvent<ContractorComponent, MapInitEvent>(OnContractorMapInit);
        SubscribeLocalEvent<ContractorMarkerComponent, MapInitEvent>(OnMarkerMapInit);

        InitializeUplink();
        InitializePortal();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateContractor();
        UpdateUplink();
        UpdatePrisoner();
        UpdatePortal();
    }

    private void UpdateContractor()
    {
        var contractorQuery = EntityQueryEnumerator<ContractorComponent>();
        while (contractorQuery.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime < comp.TryAgainTime || comp.TryAgainTime == TimeSpan.Zero)
                continue;

            comp.TryAgainTime = TimeSpan.Zero;
            SetupContracts((uid, comp));
        }
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
            ent.Comp.TryAgainTime = _gameTiming.CurTime + ent.Comp.RefreshTime;
            Log.Debug(
                "Not enough alive humanoids to generate a contract"); // add a refresh timer or sum IDK need a brain
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

    #region Helper Methods
    private EntityCoordinates GetTileInFrontOfEntity(EntityUid ent) // holy supercode someone please tell me something exists to already do this having bespoke transform code in here makes me blow up
    {
        var facingDirection = Transform(ent).LocalRotation.GetCardinalDir();

        // Calculate the tile in front of the entity
        var offset = facingDirection.ToVec();
        return ent.ToCoordinates().Offset(offset);
    }

    #endregion
}
