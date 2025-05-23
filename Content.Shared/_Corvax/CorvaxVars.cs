using Robust.Shared.Configuration;

namespace Content.Shared.Corvax.CorvaxVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
public sealed class CorvaxVars
{
    /// <summary>
    /// Responsible for turning on and off the bark system.
    /// </summary>
    public static readonly CVarDef<bool> BarksEnabled =
        CVarDef.Create("voice.barks_enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of Barks sound
    /// </summary>
    public static readonly CVarDef<float> BarksVolume =
        CVarDef.Create("voice.barks_volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
