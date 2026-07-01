using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<float> PirateEmoteCooldownSeconds =
        CVarDef.Create("pirate.emote_cooldown_seconds", 1.5f, CVar.SERVER | CVar.REPLICATED);
}
