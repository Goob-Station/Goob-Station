using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Events;
using Content.Shared._Goobstation.CCVar;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.PlayerListener;

/// <summary>
///     Records and notifies when a user has rage quit the game.
///
///     To qualify as a rage quit, the next things should be true
///     1. The character the user was playing has hit a damage threshold
///     2. The damage threshold has degraded the state of the mob (Alive->Crit, Crit->Dead, Alive->Dead)
///     2. The player has left the game in X or less amount of seconds after condition 2 became true
/// </summary>
public sealed partial class RageQuitNotifySystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IServerNetManager _network = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private EntityUid _ent;
    private bool _notify = true;
    private float _timer = 5f;

    public override void Initialize()
    {
        base.Initialize();
        InitializeDiscord();

        Subs.CVar(_cfg, GoobCVars.PlayerRageQuitNotify, value => _notify = value, invokeImmediately: true);
        Subs.CVar(_cfg, GoobCVars.PlayerRageQuitTimeThreshold, value => _timer = value, invokeImmediately: true);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<ActorComponent, MobStateChangedEvent>(OnActorMobStateChanged);

        _network.Disconnect += OnDisconnect;
    }

    public override void Shutdown()
    {
        _network.Disconnect -= OnDisconnect;
    }

    private void OnRoundStarting(RoundStartingEvent args)
    {
        _ent = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<PlayerListenerComponent>(_ent);
    }

    private void OnDisconnect(object? sender, NetChannelArgs args)
    {
        if (!_notify || !IsPendingRageQuit(args.Channel.UserId))
            return;

        var callout = GetCallout(args.Channel);
        _chat.ChatMessageToAll(ChatChannel.OOC, callout, callout, _ent, false, true, colorOverride: Color.FromHex("#fff0ff", Color.Honeydew));
        NotifyWebhook(args.Channel);
        ClearTimer(args.Channel.UserId);
    }

    private void OnActorMobStateChanged(Entity<ActorComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState == MobState.Invalid || args.NewMobState < args.OldMobState)
            return;

        var target = ent.Comp.PlayerSession;
        StartTimer(target);
    }

    private void StartTimer(ICommonSession target)
    {
        AddTimer(target);
        Timer.Spawn(TimeSpan.FromSeconds(_timer), () => ClearTimer(target));
    }

    private string GetCallout(INetChannel chan)
    {
        return Loc.GetString("rage-quit-notify", ("player", chan.UserName));
    }

    private void AddTimer(ICommonSession target)
    {
        GetPendingRageQuitList().Add(target.UserId);
    }

    private void ClearTimer(ICommonSession target)
    {
        ClearTimer(target.UserId);
    }

    private void ClearTimer(NetUserId target)
    {
        GetPendingRageQuitList().Remove(target);
    }

    private bool IsPendingRageQuit(NetUserId target)
    {
        return GetPendingRageQuitList().Contains(target);
    }

    private HashSet<NetUserId> GetPendingRageQuitList()
    {
        // This has to be a trycomp due to tests not actually creating the entity anyway
        TryComp<PlayerListenerComponent>(_ent, out var plcomp);

        return plcomp?.UserIds ?? [];
    }
}
