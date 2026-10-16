using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.RoundIntro;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.RoundIntro;

public sealed partial class RoundIntroSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IStateManager _state = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;

    private static readonly Color CrewColor = Color.FromHex("#d8c48a");
    private static readonly Color AntagColor = Color.FromHex("#d9483b");
    private static readonly TimeSpan MaxEnterWait = TimeSpan.FromSeconds(10);

    private RoundIntroOverlay? _overlay;
    private TimeSpan? _enterDeadline;
    private bool _hasIntro;
    private bool _inGame;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RoundIntroEvent>(OnRoundIntro);
        SubscribeNetworkEvent<RoundIntroCoworkersEvent>(OnCoworkers);
        SubscribeNetworkEvent<TickerJoinLobbyEvent>(OnJoinLobby);
        SubscribeLocalEvent<ClientJoinGameAttemptEvent>(OnJoinGameAttempt);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ClearOverlay();
    }

    private void OnJoinGameAttempt(ref ClientJoinGameAttemptEvent ev)
    {
        if (!_cfg.GetCVar(GoobCVars.RoundIntroEnabled) || _state.CurrentState is not LobbyState)
            return;

        ClearOverlay();

        var overlay = new RoundIntroOverlay();
        _overlay = overlay;
        _hasIntro = false;
        _inGame = false;

        overlay.FadedToBlack += () =>
        {
            if (_overlay == overlay)
                _enterDeadline = _timing.RealTime + MaxEnterWait;
        };
        overlay.Finished += () => _ui.DeferAction(() =>
        {
            if (_overlay == overlay)
                ClearOverlay();
        });

        _ui.RootControl.AddChild(overlay);
        overlay.StartFadeToBlack();
        ev.Handled = true;
    }

    private void OnRoundIntro(RoundIntroEvent ev)
    {
        if (_overlay == null || _hasIntro || _inGame)
            return;

        string? antagName = null;
        if (ev.Antag is { } antagId && _proto.TryIndex(antagId, out var antagProto))
            antagName = Loc.GetString(antagProto.Name);

        string? job = null;
        string? antagLine = null;
        var jobColor = CrewColor;

        if (ev.AntagBody)
        {
            if (antagName != null)
                job = Loc.GetString("round-intro-antag-role", ("antag", antagName));
            jobColor = AntagColor;
        }
        else
        {
            if (ev.Job is { } jobId && _proto.TryIndex(jobId, out var jobProto))
                job = Loc.GetString("round-intro-job", ("job", jobProto.LocalizedName));

            if (antagName != null)
                antagLine = Loc.GetString("round-intro-antag-hidden", ("antag", antagName));
        }

        _overlay.SetText(ev.Entity,
            Loc.GetString("round-intro-prefix"),
            ev.Name,
            job,
            jobColor,
            antagLine,
            ev.Location is { } location ? Loc.GetString("round-intro-station", ("station", location)) : null);

        _hasIntro = true;
    }

    private void OnCoworkers(RoundIntroCoworkersEvent ev)
    {
        if (_overlay == null || !_hasIntro)
            return;

        _overlay.SetCoworkersTitle(Loc.GetString(ev.Teammates ? "round-intro-teammates" : "round-intro-coworkers"));

        foreach (var coworker in ev.Coworkers)
        {
            string? job = null;
            if (coworker.Job is { } jobId && _proto.TryIndex(jobId, out var jobProto))
                job = jobProto.LocalizedName;
            else if (coworker.Antag is { } antagId && _proto.TryIndex(antagId, out var antagProto))
                job = Loc.GetString(antagProto.Name);

            _overlay.AddCoworker(coworker.Entity, coworker.Name, job);
        }
    }

    private void OnJoinLobby(TickerJoinLobbyEvent ev)
    {
        ClearOverlay();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_enterDeadline is not { } deadline || _overlay == null)
            return;

        if (!IsWorldReady() && _timing.RealTime < deadline)
            return;

        _enterDeadline = null;
        EnterGame(_overlay);
    }

    private bool IsWorldReady()
    {
        if (_player.LocalEntity is not { } ent || TerminatingOrDeleted(ent))
            return false;

        var mapId = Transform(ent).MapID;
        if (mapId == MapId.Nullspace || !_map.MapExists(mapId))
            return false;

        var eyeMap = _eye.CurrentEye.Position.MapId;
        return eyeMap == MapId.Nullspace || _map.MapExists(eyeMap);
    }

    private void EnterGame(RoundIntroOverlay overlay)
    {
        _inGame = true;
        _state.RequestStateChange<GameplayState>();
        overlay.SetPositionInParent(_ui.RootControl.ChildCount - 1);

        if (_hasIntro)
            overlay.StartIntro();
        else
            overlay.StartFadeOut();
    }

    private void ClearOverlay()
    {
        _overlay?.Orphan();
        _overlay = null;
        _hasIntro = false;
        _inGame = false;
        _enterDeadline = null;
    }
}
