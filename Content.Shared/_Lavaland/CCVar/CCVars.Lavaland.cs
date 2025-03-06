using Robust.Shared.Configuration;

// ReSharper disable once CheckNamespace
namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Should the Lavaland roundstart generation be enabled.
    /// </summary>
    public static readonly CVarDef<bool> LavalandEnabled =
        CVarDef.Create("lavaland.enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<bool> AllowDuplicatePkaModules =
        CVarDef.Create("modkit.dupes_enabled", true, CVar.REPLICATED | CVar.SERVER);
}
