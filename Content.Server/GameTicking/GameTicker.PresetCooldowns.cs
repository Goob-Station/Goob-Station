using Content.Goobstation.Common.CCVar;
using Content.Server.GameTicking.Presets;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    private readonly List<string> _presetHistory = new();

    private void RecordPresetHistory()
    {
        if (CurrentPreset == null)
            return;

        _presetHistory.Add(CurrentPreset.ID);
    }

    public int GetPresetRoundCooldown(GamePresetPrototype preset)
    {
        foreach (var entry in _cfg.GetCVar(GoobCVars.PresetRoundCooldowns).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var split = entry.Split(':', StringSplitOptions.TrimEntries);
            if (split.Length != 2 || !int.TryParse(split[1], out var rounds))
                continue;

            if (FindGamePreset(split[0])?.ID == preset.ID)
                return rounds;
        }

        return preset.RoundCooldown ?? 0;
    }

    public bool IsPresetOnRoundCooldown(GamePresetPrototype preset)
    {
        var cooldown = GetPresetRoundCooldown(preset);
        if (cooldown <= 1)
            return false;

        var start = Math.Max(0, _presetHistory.Count - (cooldown - 1));
        for (var i = start; i < _presetHistory.Count; i++)
        {
            if (_presetHistory[i] == preset.ID)
                return true;
        }

        return false;
    }
}
