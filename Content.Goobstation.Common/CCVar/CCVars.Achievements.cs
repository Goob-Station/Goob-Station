using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

public sealed partial class GoobCVars
{
    public static readonly CVarDef<bool> AchievementsEnabled =
        CVarDef.Create("achievements.enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<string> AchievementsExcludedPresets =
        CVarDef.Create("achievements.excluded_presets", "Sandbox,Deathmatch", CVar.SERVERONLY);

    public static readonly CVarDef<string> AchievementsExcludedMaps =
        CVarDef.Create("achievements.excluded_maps", "Dev,Empty,TestTeg", CVar.SERVERONLY);

    public static readonly CVarDef<float> AchievementsSaveInterval =
        CVarDef.Create("achievements.save_interval", 60f, CVar.SERVERONLY);
}
