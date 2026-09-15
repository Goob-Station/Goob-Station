using System.Linq;
using Content.Goobstation.Shared.AlertLevel;
using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Slasher;
using Content.Server.Access.Systems;
using Content.Server.Communications;
using Content.Server.GameTicking.Rules;
using Content.Server.NukeOps;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared._White.Xenomorphs;
using Content.Server.AlertLevel;
using Content.Shared.Emp;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.NukeOps;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.AlertLevel;

/// <summary>
/// Controls whether the amber alert level is unlocked.
/// </summary>
public sealed class AmberAlertSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WarDeclaredEvent>(OnWarDeclared, after: new[] { typeof(NukeopsRuleSystem) });
        SubscribeLocalEvent<EventHereticAscension>(OnHereticAscension);
        SubscribeLocalEvent<ShadowlingAscendEvent>(OnShadowlingAscend);
        SubscribeLocalEvent<SlasherAscendedEvent>(OnSlasherAscend);
        SubscribeLocalEvent<XenomorphsAnnouncedEvent>(OnXenomorphsAnnounced);
        SubscribeLocalEvent<AlertLevelSelectAttemptEvent>(OnAlertSelectAttempt);
        SubscribeLocalEvent<CommunicationsConsoleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }


    private void OnWarDeclared(ref WarDeclaredEvent ev)
    {
        if (ev.Status == WarConditionStatus.WarReady)
            UnlockAmberAlert();
    }

    private void OnHereticAscension(EventHereticAscension args) =>
        UnlockAmberAlert();

    private void OnShadowlingAscend(ShadowlingAscendEvent ev) =>
        UnlockAmberAlert(); // May as well

    private void OnSlasherAscend(SlasherAscendedEvent ev) =>
        UnlockAmberAlert();

    private void OnXenomorphsAnnounced(XenomorphsAnnouncedEvent ev) =>
        UnlockAmberAlert();

    /// <summary>
    /// Unlocks amber alert, allowing it to be manually activated from a
    /// communications console. Called when a qualifying threat occurs.
    /// </summary>
    public void UnlockAmberAlert()
    {
        var query = EntityQueryEnumerator<AlertLevelComponent>();
        while (query.MoveNext(out var station, out _))
        {
            var amber = EnsureComp<AmberAlertComponent>(station);
            if (amber.Unlocked)
                continue;

            amber.Unlocked = true;
            _radio.SendRadioMessage(station, Loc.GetString("alert-level-amber-unlocked-announcement"), amber.CommandChannel, station);
            PlayUnlockSound(amber, station);
        }
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
            AnnounceAuthorization(amber, console, idCard.Comp.FullName, "alert-level-amber-authorized-initiated-announcement");
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

        AnnounceAuthorization(amber, console, idCard.Comp.FullName, "alert-level-amber-authorized-announcement");

        return true;
    }

    private void AnnounceAuthorization(AmberAlertComponent amber, EntityUid console, string? name, string locId)
    {
        var announcement = Loc.GetString(locId,
            ("name", name ?? Loc.GetString("alert-level-amber-unknown-name")));
        _radio.SendRadioMessage(console, announcement, amber.CommandChannel, console);

        PlayUnlockSound(amber, console);
    }

    private void PlayUnlockSound(AmberAlertComponent amber, EntityUid source)
    {
        if (!IsCommandChannelUp(amber, source))
            return;

        var filter = Filter.Empty().AddWhereAttachedEntity(entity => HasCommandComms(amber, entity));
        _audio.PlayGlobal(amber.UnlockSound, filter, true);
    }

    private bool HasCommandComms(AmberAlertComponent amber, EntityUid entity)
    {
        return TryComp<WearingHeadsetComponent>(entity, out var wearing)
            && TryComp<ActiveRadioComponent>(wearing.Headset, out var radio)
            && (radio.ReceiveAllChannels || radio.Channels.Contains(amber.CommandChannel))
            && !HasComp<EmpDisabledComponent>(wearing.Headset);
    }

    private bool IsCommandChannelUp(AmberAlertComponent amber, EntityUid console)
    {
        var channel = _prototype.Index(amber.CommandChannel);
        if (channel.LongRange)
            return true;

        var mapId = Transform(console).MapID;
        var query = EntityQueryEnumerator<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var keys, out var power, out var transform))
        {
            if (transform.MapID == mapId && power.Powered && keys.Channels.Contains(amber.CommandChannel))
                return true;
        }

        return false;
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
