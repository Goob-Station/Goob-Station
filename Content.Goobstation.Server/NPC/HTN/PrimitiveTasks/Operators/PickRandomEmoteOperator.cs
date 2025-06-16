using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Server.Emoting;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PickRandomEmoteOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    [DataField]
    public string Key = "Emote";

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var uid = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent(uid, out RandomEmotesComponent? emotesComp))
            return (false, null);

        var weights = _protoMan.Index(emotesComp.Weights);
        var emote = Loc.GetString(weights.Pick(_random));
        return (true, new Dictionary<string, object>
        {
            {Key, emote}
        });
    }
}
