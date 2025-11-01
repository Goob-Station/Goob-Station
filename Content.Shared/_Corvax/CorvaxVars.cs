using Robust.Shared.Configuration;

namespace Content.Shared.Corvax.CorvaxVars;

[CVarDefs]
public sealed class CorvaxVars
{
    /// <summary>
    /// Set to true to enable voice barks and disable default speech sounds.
    /// </summary>
    public static readonly CVarDef<bool> BarksEnabled =
        CVarDef.Create("voice.barks_enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Client volume setting for barks.
    /// </summary>
    public static readonly CVarDef<float> BarksVolume =
        CVarDef.Create("voice.barks_volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
