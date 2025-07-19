using Content.Shared.Bed.Sleep;
using Content.Shared.InteractionVerbs;
using Content.Shared.Mobs.Components;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ToggleSleepingAction : InteractionAction
{
    [DataField]
    public bool WakeUp = false, Sleep = false;

    public override bool IsAllowed(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var isSleeping = deps.EntMan.HasComponent<SleepingComponent>(args.Target);
        if (!isSleeping)
            return Sleep && deps.EntMan.HasComponent<MobStateComponent>(args.Target); // Non-mobs cannot sleep

        return WakeUp;
    }

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        if (isBefore)
            args.Blackboard["sleeping"] = deps.EntMan.HasComponent<SleepingComponent>(args.Target);

        return true; // We already checked the rest in IsAllowed
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var isSleeping = deps.EntMan.HasComponent<SleepingComponent>(args.Target);
        if (args.TryGetBlackboard("sleeping", out bool wasSleeping) && wasSleeping != isSleeping)
            return false; // The target woke up/went to sleep during the do-after - sus

        if (isSleeping && WakeUp)
            return deps.EntMan.System<SleepingSystem>().TryWaking(args.Target, user: args.User);
        else if (Sleep)
            return deps.EntMan.System<SleepingSystem>().TrySleeping(args.Target);

        return false;
    }
}
