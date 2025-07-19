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
            args.Blackboard["standing"] = state.CurrentState;

        return state.CurrentState == StandingState.Standing && MakeLaying
               || state.CurrentState == StandingState.Lying && MakeStanding;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var stateSystem = deps.EntMan.System<StandingStateSystem>();

        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state)
            || args.TryGetBlackboard("standing", out StandingState oldStanding) && oldStanding != state.CurrentState)
            return false;

        // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
        if (state.CurrentState == StandingState.Lying && MakeStanding)
            return stateSystem.Stand(args.Target);
        else if (state.CurrentState == StandingState.Standing && MakeLaying)
            return stateSystem.Down(args.Target);

        return false;
    }
}
