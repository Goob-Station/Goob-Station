namespace Content.Goobstation.Common.Ingestion;

/// <summary>
/// Raised directed at the eater after finishing eating the food before it's deleted.
/// </summary>
[ByRefEvent]
public readonly record struct AfterEatingEvent(EntityUid Food)
{
    /// <summary>
    /// The UID of the food entity that was fully eaten.
    /// </summary>
    public readonly EntityUid Food = Food;
}
