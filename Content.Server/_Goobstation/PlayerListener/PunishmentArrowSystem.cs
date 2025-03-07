using System.Threading;
using Content.Server.GameTicking.Events;
using Content.Server.Pointing.Components;
using Content.Server.Popups;
using Content.Shared._Goobstation.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Systems;
using Content.Shared.Pointing;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Goobstation.PlayerListener;

// A small and confident arrow that punishes the evil, guilty, and all sorts of irreverent people.

[UsedImplicitly]
public sealed class PunishmentArrowSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private Dictionary<SessionData, int> _arrows = [];
    private HashSet<SessionData> _banned = [];
    private HashSet<CancellationTokenSource> _tokens = [];

    private int _warnThreshold = 10;
    private int _punishmentThreshold = 20;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.PunishmentArrowAngerThreshold,
            value =>
            {
                _punishmentThreshold = value;
                _warnThreshold = (value + value % 2) / 2; // Round up
            });

        SubscribeLocalEvent<PointAttemptEvent>(OnPointAttempt);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(RestartCleanup);
    }

    private void RestartCleanup(RoundRestartCleanupEvent ev)
    {
        _banned.Clear();
        _arrows.Clear();
        foreach (var token in _tokens)
        {
            token.Cancel();
        }
    }

    private void OnPointAttempt(PointAttemptEvent ev)
    {
        if (ev.Cancelled
            || !_playMan.TryGetSessionByEntity(ev.Uid, out var sesh)
            || !_mobState.IsAlive(ev.Uid))
            return;

        if (_banned.Contains(sesh.Data))
        {
            ev.Cancel();
            return;
        }

        if (!_arrows.TryGetValue(sesh.Data, out _))
        {
            _arrows[sesh.Data] = 0;
            var cts = new CancellationTokenSource();
            _tokens.Add(cts);

            Timer.Spawn(TimeSpan.FromMinutes(1), () =>  Clear(sesh.Data), cts.Token);
        }

        _arrows[sesh.Data]++;

        if (_arrows[sesh.Data] == _warnThreshold)
            _popup.PopupEntity("The arrows are tilting towards you.", ev.Uid, ev.Uid, PopupType.SmallCaution);

        if (_arrows[sesh.Data] >= _punishmentThreshold)
        {
            EnsureComp<PointingArrowAngeringComponent>(ev.Uid, out var comp);
            comp.RemainingAnger = 1;

            _banned.Add(sesh.Data);
        }
    }

    private void Clear(SessionData data)
    {
        _arrows.Remove(data);
    }
}
