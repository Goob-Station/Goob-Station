using Robust.Shared.Configuration;

// ReSharper disable once CheckNamespace
namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Should the Lavaland roundstart generation be enabled.
    /// </summary>
    public static readonly CVarDef<bool> LavalandEnabled =
        CVarDef.Create("lavaland.enabled", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
