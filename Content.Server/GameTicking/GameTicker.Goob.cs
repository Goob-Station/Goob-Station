using System.Linq;
using Content.Goobstation.Shared.Lobby;
using Content.Server.Station.Components;
using Content.Shared.GameTicking;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    private LobbyRoundInfoEvent GetLobbyRoundInfoMsg()
    {
        var preset = CurrentPreset ?? Preset;
        var modeTitle = preset == null ? string.Empty : Loc.GetString(preset.ModeTitle);

        var stations = new List<string>();
        var query = EntityQueryEnumerator<StationJobsComponent, StationSpawningComponent, MetaDataComponent>();
        while (query.MoveNext(out _, out _, out var meta))
        {
            stations.Add(meta.EntityName);
        }

        var mapName = stations.Count > 0
            ? string.Join(", ", stations)
            : _gameMapManager.GetSelectedMap()?.MapName ?? Loc.GetString("game-ticker-no-map-selected");

        var readyCount = _playerGameStatuses.Values.Count(x => x == PlayerGameStatus.ReadyToPlay);

        return new LobbyRoundInfoEvent(RoundId, mapName, modeTitle, _playerManager.PlayerCount, readyCount);
    }
}
