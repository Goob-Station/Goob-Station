using Robust.Shared.Map;

namespace Content.Server._Goobstation.AntagBase;

const string AntagMapDIR = "Maps/_Goobstation/Nonstations/antagplanet.yml";
private void OnRoundStart(RoundStartingEvent ev)
{
    var DirPath = new ResPath(AntagMapDIR);

    if (_mapLoader.TryLoadMap(DirPath, out var MapStat, out _, new DeserializationOptions { InitializeMaps = true }))
        _mapSystem.SetPaused(MapStat.Value.Comp.MapId, false);
}
