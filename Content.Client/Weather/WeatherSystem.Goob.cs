using Content.Shared.Weather;

namespace Content.Client.Weather;

public sealed partial class WeatherSystem
{
    /// <summary>
    /// DeltaV Additions: Prevent hearing weather in the lobby
    /// </summary>
    private bool CanHearWeather(EntityUid uid, WeatherData weather)
    {
        if (_playerManager.LocalEntity is not {} ent)
            return false;

        var map = Transform(uid).MapUid;
        var entMap = Transform(ent).MapUid;

        if (map == null || entMap != map)
        {
            weather.Stream = _audio.Stop(weather.Stream);
            return false;
        }

        return true;
    }
}
