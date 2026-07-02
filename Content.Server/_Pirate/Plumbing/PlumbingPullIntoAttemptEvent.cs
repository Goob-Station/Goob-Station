using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._Pirate.Plumbing;

/// <summary>
///     Raised on the DESTINATION (puller) entity when a reagent is about to be pulled into it.
///     Handlers can reduce <see cref="MaxAllowed"/> to cap the pull amount,
///     or set <see cref="Cancelled"/> to deny the pull entirely.
/// </summary>
[ByRefEvent]
public struct PlumbingPullIntoAttemptEvent(EntityUid source, string reagentPrototype, FixedPoint2 amount)
{
    /// <summary>
    ///     The source entity being pulled from.
    /// </summary>
    public EntityUid Source = source;

    /// <summary>
    ///     The reagent prototype ID being pulled.
    /// </summary>
    public string ReagentPrototype = reagentPrototype;

    /// <summary>
    ///     The amount the system wants to pull. Read-only reference value.
    /// </summary>
    public FixedPoint2 Amount = amount;

    /// <summary>
    ///     Maximum amount the destination will accept. Handlers can lower this
    ///     (but not raise it above <see cref="Amount"/>). Defaults to <see cref="Amount"/>.
    /// </summary>
    public FixedPoint2 MaxAllowed = amount;

    /// <summary>
    ///     Set to true to fully deny pulling this reagent.
    /// </summary>
    public bool Cancelled = false;
}
