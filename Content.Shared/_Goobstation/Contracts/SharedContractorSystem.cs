using System.Linq;
using Content.Shared.Mind;
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

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContractorComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ContractorComponent> ent, ref MapInitEvent args)
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

        var contracts = possibleContracts.OrderBy(x => _random.Next())
            .Take(5)
            .Select(entity => GetNetEntity(entity.Owner))
            .ToList();

        ent.Comp.Contracts = contracts;
    }
}
