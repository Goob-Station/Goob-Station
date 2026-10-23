using Content.Goobstation.Common.CCVar;
using Content.Server.GameTicking.Presets;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    public bool IsPresetDisabled(GamePresetPrototype preset)
    {
        foreach (var entry in _cfg.GetCVar(GoobCVars.DisabledGamePresets).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (FindGamePreset(entry)?.ID == preset.ID)
                return true;
        }

        return false;
    }
}
