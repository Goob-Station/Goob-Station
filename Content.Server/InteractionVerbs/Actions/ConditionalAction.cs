using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that performs one of the two (or just one) actions based on a condition.
/// </summary>
[Serializable]
public sealed partial class ConditionalAction : InteractionAction
{
    [DataField(required: true)]
    public InteractionRequirement Condition;

    [DataField("true")]
    public InteractionAction? TrueBranch;

    [DataField("false")]
    public InteractionAction? FalseBranch;

    /// <summary>
    ///     If true, CanPerform and Perform will fail when the condition results in a <c>null</c> branch.
    ///     Otherwise, null branch is equivalent to a no-op action.
    /// </summary>
    [DataField("failWhenNull")]
    public bool FailWhenNoBranch = false;

    /// <summary>
    ///     If true, the IsValid check will be delegated to the respective branch.
    ///     If the respective branch is <c>null</c>, the decision will be made based on <see cref="FailWhenNoBranch"/>
    /// </summary>
    [DataField("delegateValid")]
    public bool DelegateValidation = false;

    /// <summary>
    ///     If true, the CanPerform check will be performed before the do-after, interrupting the verb early.
    /// </summary>
    [DataField]
    public bool BeforeDelay = false;

    public override bool IsAllowed(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (!DelegateValidation)
            return true;

        var branch = Condition.IsMet(args, proto, deps) ? TrueBranch : FalseBranch;
        return branch?.IsAllowed(args, proto, deps) ?? !FailWhenNoBranch;
    }

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        if (beforeDelay && !BeforeDelay)
            return true;

        var branch = Condition.IsMet(args, proto, deps) ? TrueBranch : FalseBranch;
        return branch?.CanPerform(args, proto, beforeDelay, deps) ?? !FailWhenNoBranch;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var branch = Condition.IsMet(args, proto, deps) ? TrueBranch : FalseBranch;
        return branch?.Perform(args, proto, deps) ?? !FailWhenNoBranch;
    }
}
