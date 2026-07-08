using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.DoAfter;

namespace Content.Server.Xenobiology.HTN;

public sealed partial class HasActiveDoAfterPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField]
    public bool Invert = false;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        return _entManager.HasComponent<ActiveDoAfterComponent>(owner) ^ Invert;
    }
}
