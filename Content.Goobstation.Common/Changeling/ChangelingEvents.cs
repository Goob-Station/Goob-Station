namespace Content.Goobstation.Common.Changeling;

/// <summary>
/// Used for modifying a changeling's chemical reserves.
/// </summary>
/// <param name="Amount"> The value that will be applied to a changeling's chemical reserves. </param>
[ByRefEvent]
public record struct ChangelingModifyChemicalsEvent(float Amount);

/// <summary>
/// Used for modifying a changeling's biomass levels.
/// </summary>
/// <param name="Amount"> The value that will be applied to a changeling's biomass levels. </param>
[ByRefEvent]
public record struct ChangelingModifyBiomassEvent(float Amount);
