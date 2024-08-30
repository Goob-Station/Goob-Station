using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Represents an action performed when a verb is used successfully.
/// </summary>
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class InteractionAction
{
    /// <summary>
    ///     Invoked when the user wants to get the list of verbs that can be performed on the target, after all verb-specific checks have passed..
    ///     If this method returns false, it will not be shown to the user.
    /// </summary>
    public virtual bool IsAllowed(
        InteractionArgs args,
        InteractionVerbPrototype proto,
        VerbDependencies deps
    ) => true;

    /// <summary>
    ///     Checks whether this verb can be performed at the current moment.
    ///     If the verb has a do-after, this will be called both before and after the do-after.
    /// </summary>
    public abstract bool CanPerform(
        InteractionArgs args,
        InteractionVerbPrototype proto,
        bool beforeDelay,
        VerbDependencies deps
    );

    /// <summary>
    ///     Performs the action and returns whether it was successful.
    /// </summary>
    public abstract bool Perform(
        InteractionArgs args,
        InteractionVerbPrototype proto,
        VerbDependencies deps
    );

    /// <summary>
    ///     Provided to interaction verbs to avoid unnecessary dependency injection.
    /// </summary>
    /// <remarks>
    ///     To acquire a working instance of this class, allocate a new instance and use IoCManager.InjectDependencies().
    /// </remarks>
    public sealed class VerbDependencies
    {
        [Dependency] public readonly IEntityManager EntMan = default!;
        [Dependency] public readonly IPrototypeManager ProtoMan = default!;
        [Dependency] public readonly IRobustRandom Random = default!;
        [Dependency] public readonly IGameTiming Timing = default!;
        [Dependency] public readonly ISerializationManager Serialization = default!;
    }
}
