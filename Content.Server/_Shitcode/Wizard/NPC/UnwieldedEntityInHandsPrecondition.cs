using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.Hands.Components;
using Content.Shared.Wieldable.Components;

namespace Content.Server._Goobstation.Wizard.NPC;

public sealed partial class UnwieldedEntityInHandsPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField]
    public bool Invert;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var result = blackboard.TryGetValue(NPCBlackboard.ActiveHandEntity, out EntityUid? item, _entManager) &&
                     _entManager.TryGetComponent(item, out WieldableComponent? wieldable) && !wieldable.Wielded;

        return result ^ Invert;
    }
}
