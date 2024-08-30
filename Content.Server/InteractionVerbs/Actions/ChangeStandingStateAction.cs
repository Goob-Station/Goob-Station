using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ChangeStandingStateAction : InteractionAction
{
    [DataField]
    public bool MakeStanding, MakeLaying;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        if (isBefore)
            args.Blackboard["standing"] = state.Standing;

        return state.Standing ? MakeLaying : MakeStanding;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var stateSystem = deps.EntMan.System<StandingStateSystem>();
        var isDown = stateSystem.IsDown(args.Target);

        if (args.TryGetBlackboard("standing", out bool wasStanding) && wasStanding != !isDown)
            return false; // The target changed its standing state during the do-after - sus

        // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
        if (isDown && MakeStanding)
            return stateSystem.Stand(args.Target);
        else if (!isDown && MakeLaying)
            return stateSystem.Down(args.Target);

        return false;
    }
}
