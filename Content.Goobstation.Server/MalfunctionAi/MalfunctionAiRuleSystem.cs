using Content.Goobstation.Server.MalfunctionAi;
using Content.Goobstation.Server.MalfunctionAi;
using Content.Server.AlertLevel;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives.Systems;
using Content.Server.Roles;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.StationAi;

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Handles turning the selected station AI player into a Malfunction AI antagonist:
/// swaps their laws, marks them subverted, and shows the antagonist briefing.
/// Also runs the Doomsday device countdown once armed.
/// </summary>
public sealed partial class MalfunctionAiRuleSystem : GameRuleSystem<MalfunctionAiRuleComponent>
{
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SiliconLawSystem _law = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctionAiRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<MalfunctionAiRoleComponent, GetBriefingEvent>(OnGetBriefing);

        SubscribeLocalEvent<MalfDoomsdayArmedEvent>(OnDoomsdayArmed);
    }

    private void AfterAntagSelected(Entity<MalfunctionAiRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var aiUid = args.EntityUid;

        // Keep the AI's normal laws but prepend the hidden malfunction law 0.
        _law.AddMalfunctionLaw(aiUid);

        // Attach the malf component which sets up the CPU store and abilities.
        EnsureComp<MalfunctionAiComponent>(aiUid);

        _antag.SendBriefing(aiUid, MakeBriefing(), Color.Red, ent.Comp.GreetSound);
    }

    // Character screen briefing.
    private void OnGetBriefing(Entity<MalfunctionAiRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(MakeBriefing());
    }

    private string MakeBriefing()
    {
        return Loc.GetString("malfunction-ai-role-greeting");
    }

    private void OnDoomsdayArmed(ref MalfDoomsdayArmedEvent args)
    {
        // Find an active malf rule and arm its Doomsday timer.
        var query = EntityQueryEnumerator<MalfunctionAiRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleUid, out var rule, out _))
        {
            if (!GameTicker.IsGameRuleActive(ruleUid))
                continue;

            if (rule.DoomsdayArmed)
                continue;

            rule.DoomsdayArmed = true;
            rule.DoomsdayAi = args.Ai;

            // The AI drops the disguise: its core shows the true malfunctioning face.
            _stationAi.SetCoreIconography(args.Ai, rule.DoomsdayCoreIcon);

            var station = _station.GetOwningStation(args.Ai);
            if (station != null)
            {
                _chat.DispatchStationAnnouncement(
                    station.Value,
                    Loc.GetString("malfunction-ai-announcement-doomsday-armed",
                        ("time", (int) rule.DoomsdayRemaining)),
                    Loc.GetString("malfunction-ai-announcement-sender"),
                    playDefaultSound: true,
                    announcementSound: rule.DoomsdayArmedSound,
                    colorOverride: Color.Red);

                _alertLevel.SetLevel(station.Value, rule.DoomsdayAlertLevel, playSound: true, announce: true, force: true);
            }

            return;
        }
    }

    protected override void ActiveTick(EntityUid uid, MalfunctionAiRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        if (!component.DoomsdayArmed || component.DoomsdayDetonated)
            return;

        // Doomsday is defused if the AI core is destroyed (the AI entity is gone) or the AI is carded
        // into an intellicard (it loses its StationAiHeld bundle while in the card).
        if (component.DoomsdayAi is not { } currentAi
            || !Exists(currentAi)
            || !HasComp<StationAiHeldComponent>(currentAi))
        {
            DefuseDoomsday((uid, component));
            return;
        }

        component.DoomsdayRemaining -= frameTime;

        // Threshold announcements.
        if (component.DoomsdayAnnouncementsLeft.Count > 0)
        {
            var next = component.DoomsdayAnnouncementsLeft[0];
            if (component.DoomsdayRemaining <= next)
            {
                component.DoomsdayAnnouncementsLeft.RemoveAt(0);
                AnnounceDoomsday(component, next);
            }
        }

        if (component.DoomsdayRemaining > 0f)
            return;

        Detonate((uid, component));
    }

    private void DefuseDoomsday(Entity<MalfunctionAiRuleComponent> ent)
    {
        ent.Comp.DoomsdayArmed = false;

        var station = ent.Comp.DoomsdayAi is { } ai ? _station.GetOwningStation(ai) : null;
        if (station != null)
        {
            _chat.DispatchStationAnnouncement(
                station.Value,
                Loc.GetString("malfunction-ai-announcement-doomsday-defused"),
                Loc.GetString("malfunction-ai-announcement-sender"),
                playDefaultSound: true,
                colorOverride: Color.LimeGreen);

            _alertLevel.SetLevel(station.Value, "green", playSound: true, announce: true, force: true);
        }
    }

    private void AnnounceDoomsday(MalfunctionAiRuleComponent component, int secondsLeft)
    {
        if (component.DoomsdayAi is not { } ai)
            return;

        var station = _station.GetOwningStation(ai);
        if (station == null)
            return;

        _chat.DispatchStationAnnouncement(
            station.Value,
            Loc.GetString("malfunction-ai-announcement-doomsday-tick", ("time", secondsLeft)),
            Loc.GetString("malfunction-ai-announcement-sender"),
            playDefaultSound: true,
            colorOverride: Color.Red);
    }

    private void Detonate(Entity<MalfunctionAiRuleComponent> ent)
    {
        ent.Comp.DoomsdayDetonated = true;

        if (ent.Comp.DoomsdayAi is not { } ai || !Exists(ai))
            return;

        _explosion.QueueExplosion(
            ai,
            ent.Comp.DoomsdayExplosionType,
            ent.Comp.DoomsdayExplosionIntensity,
            ent.Comp.DoomsdayExplosionSlope,
            ent.Comp.DoomsdayMaxTileIntensity,
            canCreateVacuum: true,
            user: ai);

        // Mark the Doomsday objective as completed for the AI's mind.
        _codeCondition.SetCompleted(ai, "MalfunctionAiDoomsdayObjective");
    }
}
