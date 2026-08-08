using System.Linq;
using Content.Server.Access.Systems;
using Content.Server.Communications;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Server.AlertLevel;

/// <summary>
/// Goobstation.
/// Controls whether the amber alert level is unlocked.
/// </summary>
public sealed class AmberAlertSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AmberAlertThreatEvent>(OnThreat);
        SubscribeLocalEvent<AlertLevelSelectAttemptEvent>(OnAlertSelectAttempt);
        SubscribeLocalEvent<CommunicationsConsoleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnThreat(AmberAlertThreatEvent ev) =>
        UnlockAmberAlert();

    /// <summary>
    /// Unlocks amber alert, allowing it to be manually activated from a
    /// communications console. Called when a qualifying threat occurs.
    /// </summary>
    public void UnlockAmberAlert()
    {
        var query = EntityQueryEnumerator<AlertLevelComponent>();
        while (query.MoveNext(out var station, out _))
            EnsureComp<AmberAlertComponent>(station).Unlocked = true;
    }

    private void OnAlertSelectAttempt(ref AlertLevelSelectAttemptEvent ev)
    {
        var amber = EnsureComp<AmberAlertComponent>(ev.Station);
        if (ev.Level != amber.AmberLevel)
            return;

        if (!amber.Unlocked)
        {
            _popup.PopupEntity(Loc.GetString("alert-level-amber-locked"), ev.Console, ev.User, PopupType.MediumCaution);
            ev.Cancelled = true;
        }
    }

    private void OnGetVerbs(Entity<CommunicationsConsoleComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var station = _station.GetOwningStation(ent.Owner);
        if (station == null
            || !TryComp<AmberAlertComponent>(station, out var amber)
            || amber.Unlocked)
            return;

        var user = args.User;
        var console = ent.Owner;
        var stationUid = station.Value;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("alert-level-amber-verb-text"),
            Message = Loc.GetString("alert-level-amber-verb-message"),
            Priority = -1,
            Act = () =>
            {
                if (!TryComp<AmberAlertComponent>(stationUid, out var a)
                    || a.Unlocked
                    || !TryAuthorizeAmber(a, user, console))
                    return;

                a.Unlocked = true;
                _popup.PopupEntity(Loc.GetString("alert-level-amber-unlocked"), console, user, PopupType.Medium);
            },
        });
    }

    /// <summary>
    /// Runs the two-card command authorization.
    /// </summary>
    private bool TryAuthorizeAmber(AmberAlertComponent amber, EntityUid user, EntityUid console)
    {
        ExpirePending(amber);

        if (!_idCard.TryFindIdCard(user, out var idCard))
        {
            _popup.PopupEntity(Loc.GetString("alert-level-amber-no-id"), console, user, PopupType.MediumCaution);
            return false;
        }

        var tags = _accessReader.FindAccessTags(idCard);
        var isCommandHead = amber.InitiatorAccess.Any(tags.Contains);
        var isCommand = isCommandHead || tags.Contains(amber.CommandAccess);

        // First authorization must come from a Captain or Head of Security ID.
        if (amber.PendingCard == null)
        {
            if (!isCommandHead)
            {
                _popup.PopupEntity(Loc.GetString("alert-level-amber-needs-command"), console, user, PopupType.MediumCaution);
                return false;
            }

            amber.PendingCard = idCard.Owner;
            amber.PendingExpiry = _timing.CurTime + amber.PendingTimeout;
            _popup.PopupEntity(Loc.GetString("alert-level-amber-first-swipe"), console, user, PopupType.Medium);
            return false;
        }

        // Second authorization must be a different ID card
        if (amber.PendingCard == idCard.Owner)
        {
            _popup.PopupEntity(Loc.GetString("alert-level-amber-same-id"), console, user, PopupType.MediumCaution);
            return false;
        }

        // ...with command access.
        if (!isCommand)
        {
            _popup.PopupEntity(Loc.GetString("alert-level-amber-needs-second-command"), console, user, PopupType.MediumCaution);
            return false;
        }

        amber.PendingCard = null;
        amber.PendingExpiry = null;
        return true;
    }

    private void ExpirePending(AmberAlertComponent amber)
    {
        if (amber.PendingExpiry != null && _timing.CurTime > amber.PendingExpiry)
        {
            amber.PendingCard = null;
            amber.PendingExpiry = null;
        }
    }
}

/// <summary>
/// Goobstation.
/// Raised when a player attempts to select an alert level from a communications
/// console, before the level is actually changed. Cancel to prevent the change.
/// </summary>
[ByRefEvent]
public record struct AlertLevelSelectAttemptEvent(EntityUid Station, EntityUid Console, EntityUid User, string Level)
{
    public bool Cancelled;
}

/// <summary>
/// Goobstation.
/// Broadcast when a major station threat manifests (war declaration, slasher/heretic/shadowling
/// ascension, xenomorph outbreak, ...). The amber alert system listens for this to unlock the
/// amber alert level for manual activation from a communications console.
/// </summary>
public sealed class AmberAlertThreatEvent : EntityEventArgs { }
