using System.Threading;
using System.Threading.Tasks;
using Content.Server.Buckle.Systems;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared._Goobstation.MULE.Components;


namespace Content.Server._Goobstation.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class UnbuckleCrateOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BuckleSystem _buckleSystem = default!;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _buckleSystem = sysManager.GetEntitySystem<BuckleSystem>();
    }
    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent<MuleComponent>(owner, out var mule))
            return (false, null);

        var crate = mule.CurrentObject;

        if(crate == EntityUid.Invalid)
            return (false, null);

        if (!_buckleSystem.TryUnbuckle(crate, owner))
        {
            return (false, null);
        }

        return (true, new Dictionary<string, object>()
        {
        });
    }
}
