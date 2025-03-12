using Robust.Shared.Configuration;

namespace Content.Shared._White;

[CVarDefs]
public sealed class WhiteCVars
{
    #region Shoving
    /// <summary>
    /// Shove range multiplier.
    /// </summary>
    public static readonly CVarDef<float> ShoveRange =
        CVarDef.Create("game.shove_range", 0.5f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// Shove speed multiplier, does not affect range.
    /// </summary>
    public static readonly CVarDef<float> ShoveSpeed =
        CVarDef.Create("game.shove_speed", 5f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// How much should the mass difference affect shove range & speed.
    /// </summary>
    public static readonly CVarDef<float> ShoveMassFactor =
        CVarDef.Create("game.shove_mass_factor", 5f, CVar.SERVER | CVar.ARCHIVE);
    #endregion
}