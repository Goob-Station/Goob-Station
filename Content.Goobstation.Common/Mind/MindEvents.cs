namespace Content.Goobstation.Common.Mind;

/// <summary>
/// Used to see if the target is valid for an antagonist's target objective.
/// </summary>
/// <param name="IsSilicon"> Set to true if the target is a silicon. </param>
/// <param name="IsChangeling"> Set to true if the target is a changeling. </param>
[ByRefEvent]
public record struct GetAntagSelectionBlockerEvent(
    bool IsSilicon = false,
    bool IsChangeling = false);
