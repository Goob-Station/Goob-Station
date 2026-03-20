using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[CVarDefs]
public sealed class ThunderdomeCVars
{
    public static readonly CVarDef<bool> ThunderdomeEnabled =
        CVarDef.Create("thunderdome.enabled", true, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> ThunderdomeRefill =
        CVarDef.Create("thunderdome.refill", true, CVar.SERVER | CVar.REPLICATED);

    // CorvaxGoob-Thunderdome-start
    [CVarControl(AdminFlags.Admin, min: 10, max: 180)]
    public static readonly CVarDef<int> ActivationDelay =
        CVarDef.Create("thunderdome.activation_delay", Random.Shared.Next(50, 91), CVar.SERVER | CVar.REPLICATED | CVar.NOTIFY);

    public static readonly CVarDef<bool> ActivationDelayEnabled =
        CVarDef.Create("thunderdome.activation_delay_enabled", false, CVar.SERVER | CVar.REPLICATED);
    // CorvaxGoob-Thunderdome-end
}
