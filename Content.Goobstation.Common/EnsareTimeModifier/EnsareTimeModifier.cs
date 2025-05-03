namespace Content.Goobstation.Common.EnsareTimeModifier;

/// <summary>
/// Raised on an entity to change the target for a color flash effect.
/// </summary>
[ByRefEvent]
public record struct GetEnsareTimeModifier()
{
    public float FreeTime = 1f;
}
