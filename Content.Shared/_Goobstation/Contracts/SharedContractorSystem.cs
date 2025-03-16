using System.Linq;
using Content.Shared.Coordinates;
using Content.Shared.Mind;
using Content.Shared.Pinpointer;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Random;

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

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContractorComponent, MapInitEvent>(OnContractorMapInit);
        SubscribeLocalEvent<ContractorMarkerComponent, MapInitEvent>(OnMarkerMapInit);
    }

    private void OnMarkerMapInit(Entity<ContractorMarkerComponent> ent, ref MapInitEvent args)
    {
        var entitiesIntersecting = _lookupSystem.GetEntitiesIntersecting(ent.Owner.ToCoordinates());
        foreach (var entity in entitiesIntersecting)
        {
            if(!TryComp<NavMapBeaconComponent>(entity, out var navMapBeaconComponent))
                continue;

            if (navMapBeaconComponent.DefaultText == null)
                continue;

            ent.Comp.Name = navMapBeaconComponent.DefaultText;
            break;
        }
    }

    private void OnContractorMapInit(Entity<ContractorComponent> ent, ref MapInitEvent args)
    {
        SetupContracts(ent);
    }

    private void SetupContracts(Entity<ContractorComponent> ent)
    {
        if(_net.IsClient) // I really feel like this is the really lazy way of doing this instead of moving it to server but... alas we are here.
            return;

        // get ready for the craziest linq maxing
        var possibleContracts = _mindSystem.GetAliveHumans();

        foreach (var humanoid in possibleContracts)
        foreach (var mindRole in humanoid.Comp.MindRoles)
            if (!TryComp<MindRoleComponent>(mindRole, out var mindRoleComp) || mindRoleComp.Antag || humanoid.Owner == ent.Owner)
                possibleContracts.Remove(humanoid);

        var targets = possibleContracts.OrderBy(x => _random.Next())
            .Take(5)
            .Select(entity => GetNetEntity(entity.Owner))
            .ToList();

        var query = EntityQueryEnumerator<ContractorMarkerComponent>();
        var markerList = new List<NetEntity>();

        while (query.MoveNext(out var uid, out _))
        {
            markerList.Add(GetNetEntity(uid));
        }

        foreach (var target in targets)
        {
            ent.Comp.Contracts.Add(target, markerList.OrderBy(x => _random.Next()).Take(3).ToList());
        }
    }
}
