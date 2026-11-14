using System.Threading;
using Content.Shared.Database;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.RoundEnd;

public sealed partial class RoundEndSystem
{
    private bool _bypassRequestBlockers;

    public bool DelayRoundEndCountdown(TimeSpan delay)
    {
        if (_countdownTokenSource == null || ExpectedCountdownEnd is not { } end)
            return false;

        _countdownTokenSource.Cancel();
        _countdownTokenSource = new CancellationTokenSource();

        ExpectedCountdownEnd = end + delay;
        var remaining = ExpectedCountdownEnd.Value - _gameTiming.CurTime;
        if (remaining < TimeSpan.Zero)
            remaining = TimeSpan.Zero;

        Timer.Spawn(remaining, _shuttle.DockEmergencyShuttle, _countdownTokenSource.Token);
        RaiseLocalEvent(RoundEndSystemChangedEvent.Default);
        return true;
    }

    public void ForceRequestRoundEnd(TimeSpan? countdownTime, EntityUid? requester)
    {
        _bypassRequestBlockers = true;
        try
        {
            if (countdownTime is { } time)
                RequestRoundEnd(time, requester, false);
            else
                RequestRoundEnd(requester, false);
        }
        finally
        {
            _bypassRequestBlockers = false;
        }
    }

    private bool IsRoundEndRequestBlocked(EntityUid? requester)
    {
        if (_bypassRequestBlockers)
            return false;

        var ev = new RoundEndRequestAttemptEvent(requester);
        RaiseLocalEvent(ref ev);

        if (!ev.Cancelled)
            return false;

        if (requester != null)
            _adminLogger.Add(LogType.ShuttleCalled, LogImpact.High, $"Shuttle call by {ToPrettyString(requester.Value):user} was blocked");
        else
            _adminLogger.Add(LogType.ShuttleCalled, LogImpact.High, $"Shuttle call was blocked");

        if (ev.Message != null)
        {
            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString(ev.Message),
                Loc.GetString(ev.Sender ?? "round-end-system-shuttle-sender-announcement"),
                false,
                colorOverride: ev.Color ?? Color.Gold);
            _audio.PlayGlobal(BlockedSound, Filter.Broadcast(), true);
        }

        ActivateCooldown();
        RaiseLocalEvent(RoundEndSystemChangedEvent.Default);
        return true;
    }

    private static readonly SoundSpecifier BlockedSound = new SoundPathSpecifier("/Audio/Announcements/shuttlerecalled.ogg");
}

[ByRefEvent]
public record struct RoundEndRequestAttemptEvent(EntityUid? Requester)
{
    public bool Cancelled;
    public string? Message;
    public string? Sender;
    public Color? Color;

    public void Block(string message, string? sender = null, Color? color = null)
    {
        if (Cancelled)
            return;

        Cancelled = true;
        Message = message;
        Sender = sender;
        Color = color;
    }
}
