using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[CVarDefs]
public sealed class ThunderdomeCVars
{
    public static readonly CVarDef<bool> ThunderdomeEnabled =
        CVarDef.Create("thunderdome.enabled", true, CVar.SERVER | CVar.REPLICATED);
}
