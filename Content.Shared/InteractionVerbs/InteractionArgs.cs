using System.Diagnostics.CodeAnalysis;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs;

public sealed partial class InteractionArgs
{
    public EntityUid User, Target;
    public EntityUid? Used;
    public bool CanAccess, CanInteract;

    /// <summary>
    ///     A float value between 0 and positive infinity that indicates how much stronger the user
    ///     is compared to the target in terms of contests allowed for this verb. 1.0 means no advantage or disadvantage.
    /// </summary>
    /// <remarks>Can be null, which means it's not calculated yet. That can happen before the user attempts to perform the verb.</remarks>
    public float? ContestAdvantage;

    /// <summary>
    ///     A dictionary for actions and requirements to store data between different execution stages.
    ///     For instance, an action can cache some data in its CanPerform check and later use it in Perform.
    /// </summary>
    /// <remarks>
    ///     Non-action classes are highly not recommended to write anything to this dictionary - it can easily lead to errors.
    /// </remarks>
    public Dictionary<string, object> Blackboard => _blackboardField ??= new(3);
    private Dictionary<string, object>? _blackboardField; // null by default, allocated lazily (only if actually needed)

    public InteractionArgs(EntityUid user, EntityUid target, EntityUid? used, bool canAccess, bool canInteract, float? contestAdvantage)
    {
        User = user;
        Target = target;
        Used = used;
        CanAccess = canAccess;
        CanInteract = canInteract;
        ContestAdvantage = contestAdvantage;
    }

    public InteractionArgs(InteractionArgs other) : this(other.User, other.Target, other.Used, other.CanAccess, other.CanInteract, other.ContestAdvantage) {}

    public static InteractionArgs From<T>(GetVerbsEvent<T> ev) where T : Verb => new(ev.User, ev.Target, ev.Using, ev.CanAccess, ev.CanInteract, null);

    /// <summary>
    ///     Tries to get a value from the blackboard as an instance of a specific type.
    /// </summary>
    public bool TryGetBlackboard<T>(string key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (_blackboardField == null || !_blackboardField.TryGetValue(key, out var maybeValue))
            return false;

        // Cannot use a type check here. If someone fucks up, it's gonna be on them.
        value = (T?) maybeValue;
        return value != null;
    }
}
