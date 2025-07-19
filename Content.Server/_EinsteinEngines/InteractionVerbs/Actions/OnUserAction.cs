using Content.Shared.InteractionVerbs;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     A special proxy action that swaps the target and the user for the proxied action.
///     This effectively means that in most cases the proxied action will be applied to the user even if it's meant for target.
/// </summary>
[Serializable]
public sealed partial class OnUserAction : InteractionAction
{
    [DataField(required: true)]
    public InteractionAction Action = default!;

    private InteractionArgs Swap(InteractionArgs args)
    {
        return new InteractionArgs(args)
        {
            Target = args.User,
            User = args.Target
        };
    }

    public override bool IsAllowed(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return Action.IsAllowed(Swap(args), proto, deps);
    }

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        return Action.CanPerform(Swap(args), proto, beforeDelay, deps);
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return Action.Perform(Swap(args), proto, deps);
    }
}
