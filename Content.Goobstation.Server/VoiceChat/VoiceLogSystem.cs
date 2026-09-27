using System.Collections.Concurrent;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared.Administration;
using Content.Shared.Verbs;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceLogSystem : EntitySystem
{
    [Dependency] private readonly VoiceLogManager _logs = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private static readonly SpriteSpecifier VerbIcon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/examine.svg.192dpi.png"));

    private readonly ConcurrentQueue<Action> _completed = new();
    private bool _enabled;
    private int _keepRounds;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.VoiceLogEnabled, value => _enabled = value, true);
        Subs.CVar(_cfg, GoobCVars.VoiceLogRounds, value => _keepRounds = value, true);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        while (_completed.TryDequeue(out var action))
        {
            action();
        }
    }

    public void Record(ICommonSession session, VoiceWebFrame frame, VoiceLogFlags flags, string name, string channel)
    {
        if (!_enabled)
            return;

        _logs.Record(
            _gameTicker.RoundId,
            session.UserId.UserId,
            session.Name,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            frame.Sequence,
            flags,
            frame.Raw,
            name,
            channel);
    }

    public void OpenViewer(ICommonSession admin, Guid? user = null)
    {
        _eui.OpenEui(new VoiceLogsEui(user), admin);
    }

    public void OnMainThread<T>(Task<T> task, Action<T> callback)
    {
        _ = Forward(task, callback);
    }

    private async Task Forward<T>(Task<T> task, Action<T> callback)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            _completed.Enqueue(() => callback(result));
        }
        catch (Exception e)
        {
            Log.Error($"Voice log read failed: {e}");
        }
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        if (_enabled)
            _logs.StartRound(ev.Id, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), _keepRounds);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var admin) ||
            !TryComp<ActorComponent>(args.Target, out var target) ||
            !_admin.HasAdminFlag(admin.PlayerSession, AdminFlags.Logs))
        {
            return;
        }

        var adminSession = admin.PlayerSession;
        var user = target.PlayerSession.UserId.UserId;
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("voice-logs-verb"),
            Category = VerbCategory.Admin,
            Icon = VerbIcon,
            Act = () => OpenViewer(adminSession, user),
        });
    }
}
