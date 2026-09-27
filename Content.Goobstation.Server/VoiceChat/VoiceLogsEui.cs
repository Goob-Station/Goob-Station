using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Shared.VoiceChat;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.Eui;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceLogsEui : BaseEui
{
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly VoiceLogManager _logs = default!;

    private readonly VoiceLogSystem _system;
    private readonly Guid? _initialUser;

    private List<VoiceLogRound> _rounds = new();
    private List<VoiceLogSpeaker> _speakers = new();
    private int _round;
    private bool _loading = true;

    public VoiceLogsEui(Guid? initialUser)
    {
        IoCManager.InjectDependencies(this);
        _system = _entities.System<VoiceLogSystem>();
        _initialUser = initialUser;
    }

    public override void Opened()
    {
        base.Opened();

        _admin.OnPermsChanged += OnPermsChanged;
        LoadRound(_entities.System<GameTicker>().RoundId, _initialUser);
    }

    public override void Closed()
    {
        base.Closed();

        _admin.OnPermsChanged -= OnPermsChanged;
    }

    public override EuiStateBase GetNewState()
    {
        return new VoiceLogsEuiState
        {
            Rounds = _rounds,
            RoundId = _round,
            Speakers = _speakers,
            Loading = _loading,
        };
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!_admin.HasAdminFlag(Player, AdminFlags.Logs))
            return;

        switch (msg)
        {
            case VoiceLogsSelectRoundMessage select:
                LoadRound(select.RoundId, null);
                break;
            case VoiceLogsRefreshMessage:
                LoadRound(_round, null);
                break;
            case VoiceLogsRequestTrackMessage track:
                SendTrack(track.RoundId, track.UserId);
                break;
            case VoiceLogsRequestAudioMessage audio:
                _system.OnMainThread(_logs.ReadAudio(audio.RoundId, audio.UserId, audio.FirstSegment, audio.LastSegment), payloads =>
                {
                    if (!IsShutDown)
                        SendMessage(new VoiceLogsAudioMessage(audio.RoundId, audio.UserId, audio.FirstSegment, payloads));
                });
                break;
        }
    }

    private void LoadRound(int round, Guid? addUser)
    {
        _loading = true;
        StateDirty();

        var current = _entities.System<GameTicker>().RoundId;
        _system.OnMainThread(LoadRoundData(round, current), result =>
        {
            if (IsShutDown)
                return;

            _rounds = result.Rounds;
            _speakers = result.Speakers;
            _round = round;
            _loading = false;
            StateDirty();

            if (addUser is { } user)
                SendTrack(round, user);
        });
    }

    private async Task<(List<VoiceLogRound> Rounds, List<VoiceLogSpeaker> Speakers)> LoadRoundData(int round, int current)
    {
        var rounds = await _logs.ListRounds(current);
        if (rounds.All(entry => entry.RoundId != current))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            rounds.Insert(0, new VoiceLogRound(current, now, now, true));
        }

        var speakers = await _logs.ListSpeakers(round);
        return (rounds, speakers);
    }

    private void SendTrack(int round, Guid user)
    {
        _system.OnMainThread(_logs.ReadTrack(round, user), track =>
        {
            if (!IsShutDown && track is { } found)
                SendMessage(new VoiceLogsTrackMessage(round, user, found.Username, found.Segments));
        });
    }

    private void OnPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.Player == Player && !_admin.HasAdminFlag(Player, AdminFlags.Logs))
            Close();
    }
}
