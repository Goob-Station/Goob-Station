using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared._Reserve.ERT.SendShuttlePrototype;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server._Reserve.ERT.SendShuttleSystem;

public sealed class SendShuttle : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEntitySystemManager _system = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public void SpawnShuttle(string shuttletype, bool playAnnonce)
    {
        var protos = _prototypeManager;
        var shuttleProto = protos.Index<SendShuttlePrototype>(shuttletype);
        var playAnnounce = shuttleProto.ForcedAnnounce ? shuttleProto.DefaultIsAnnounce : playAnnonce;


        if (shuttleProto.IsLoadGrid)
            SpawnMapAndGrid(shuttleProto);

        if (playAnnounce)
        {
            if (shuttleProto.IsSetAlertLevel)
                SetAlertLevel(shuttleProto);

            if (shuttleProto.IsPlayAudio)
                PlayAudioAnnonce(shuttleProto);

            WriteAnnonce(shuttleProto);
        }
    }

    private void SpawnMapAndGrid(SendShuttlePrototype proto)
    {
        var mapId = _mapManager.CreateMap();
        _metaSystem.SetEntityName(
            _mapManager.GetMapEntityId(mapId),
            Loc.GetString("sent-shuttle-map-name")
        );

        var girdOptions = new MapLoadOptions
        {
            Offset = new Vector2(0, 0),
            Rotation = Angle.FromDegrees(0)
        };
        _map.Load(mapId, proto.GridPath, girdOptions);
    }

    private void PlayAudioAnnonce(SendShuttlePrototype proto)
    {
        var filter = Filter.Empty().AddAllPlayers(_playerManager);

        var audioOption = AudioParams.Default;
        audioOption = audioOption.WithVolume(proto.Volume);

        _sound.PlayAdminGlobal(filter, proto.AudioPath, audioOption, true);
    }

    private void WriteAnnonce(SendShuttlePrototype proto)
    {
        _system.GetEntitySystem<ChatSystem>().DispatchGlobalAnnouncement(
            Loc.GetString($"{proto.AnnouncementText}"),
            Loc.GetString($"{proto.AnnouncerText}"),
            playSound: proto.IsPlayAudioFromAnnouncement,
            colorOverride: proto.AnnounceColor
        );
    }

    private void SetAlertLevel(SendShuttlePrototype proto)
    {
        var stationUids = _system.GetEntitySystem<StationSystem>().GetStations();

        foreach (var stationUid in stationUids)
        {
            _alertLevel.SetLevel(stationUid, proto.AlertLevelCode,
                false, true, true, true);
        }
    }
}
