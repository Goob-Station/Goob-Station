using System.Linq;
using Content.Server._White.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Nuke;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._White.GameTicking.Rules;

public sealed class XenomorphsRuleSystem : GameRuleSystem<XenomorphsRuleComponent>
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NukeCodePaperSystem _nukeCodePaper = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphsRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);

        SubscribeLocalEvent<XenomorphComponent, ComponentInit>(OnXenomorphInit);
        SubscribeLocalEvent<XenomorphComponent, BeforeXenomorphEvolutionEvent>(BeforeXenomorphEvolution);
        SubscribeLocalEvent<XenomorphComponent, AfterXenomorphEvolutionEvent>(AfterXenomorphEvolution);

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
    }

    private void AfterAntagEntitySelected(
        EntityUid uid,
        XenomorphsRuleComponent component,
        AfterAntagEntitySelectedEvent args
    )
    {
        if (args.Session == null || !Exists(args.EntityUid))
            return;

        component.Xenomorphs.Add(args.EntityUid);
        component.AnnouncementTime ??= _timing.CurTime + _random.Next(component.MinTimeToAnnouncement, component.MaxTimeToAnnouncement);
    }

    private void OnXenomorphInit(EntityUid uid, XenomorphComponent component, ComponentInit args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var xenomorphsRule, out _))
        {
            xenomorphsRule.Xenomorphs.Add(uid);
        }
    }

    private void BeforeXenomorphEvolution(
        EntityUid uid,
        XenomorphComponent component,
        BeforeXenomorphEvolutionEvent args
    )
    {
        if (!_protoManager.TryIndex(args.Caste, out var cast) || cast.MaxCount == 0)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var xenomorphsRule, out _))
        {
            if (!xenomorphsRule.Xenomorphs.Contains(uid)
                || GetXenomorphs(xenomorphsRule, args.Caste).Count >= cast.MaxCount
                || cast.NeedCasteDeath != null && GetXenomorphs(xenomorphsRule, cast.NeedCasteDeath).Count > 0)
                continue;

            return;
        }

        _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-no-cast-slot", ("caste", Loc.GetString(cast.Name))), uid, uid);
        args.Cancel();
    }

    private void AfterXenomorphEvolution(
        EntityUid uid,
        XenomorphComponent component,
        AfterXenomorphEvolutionEvent args
    )
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var xenomorphsRule, out _))
        {
            if (xenomorphsRule.Xenomorphs.Remove(uid))
                xenomorphsRule.Xenomorphs.Add(args.EvolvedInto);
        }
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        if (ev.OwningStation == null)
            return;

        var correctStation = false;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var xenomorphs, out _))
        {
            foreach (var grid in GetStationGrids())
            {
                if (ev.OwningStation != grid)
                    continue;

                xenomorphs.WinType = WinType.CrewMinor;
                xenomorphs.WinConditions.Add(WinCondition.NukeExplodedOnStation);
                ForceEndSelf(uid);
                correctStation = true;
            }
        }

        if (correctStation)
            _roundEnd.EndRound();
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New is not GameRunLevel.PostRound)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var xenomorphs, out _))
        {
            OnRoundEnd(xenomorphs);
            ForceEndSelf(uid);
        }
    }

    private void OnRoundEnd(XenomorphsRuleComponent component)
    {
        if (component.WinType != WinType.XenoMinor)
            return;

        var centcomms = _emergencyShuttle.GetCentcommMaps();
        var station = GetStationGrids();

        var xenomorphs = GetXenomorphs(component);
        foreach (var xenomorph in xenomorphs)
        {
            var xform = Transform(xenomorph);
            if (xform.MapUid == null || !centcomms.Contains(xform.MapUid.Value))
                continue;

            component.WinType = WinType.XenoMajor;
            component.WinConditions.Add(WinCondition.XenoInfiltratedOnCentCom);
            break;
        }

        var nukeQuery = AllEntityQuery<NukeComponent, TransformComponent>();
        while (nukeQuery.MoveNext(out _, out var xform))
        {
            if (xform.MapUid == null || !station.Contains(xform.MapUid.Value))
                continue;

            component.WinType = WinType.CrewMinor;
            component.WinConditions.Add(WinCondition.NukeActiveInStation);
            break;
        }
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args
        )
    {
        var winText = Loc.GetString($"xenomorphs-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        foreach (var cond in component.WinConditions)
        {
            var text = Loc.GetString($"xenomorphs-cond-{cond.ToString().ToLower()}");
            args.AddLine(text);
        }
    }

    protected override void Started(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
    )
    {
        base.Started(uid, component, gameRule, args);

        component.NextCheck = _timing.CurTime + component.CheckDelay;
    }

    protected override void ActiveTick(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        float frameTime
    )
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (!component.AnnouncementTime.HasValue || component.NextCheck > _timing.CurTime)
            return;

        component.NextCheck = _timing.CurTime + component.CheckDelay;

        if (!component.Announced && component.AnnouncementTime <= _timing.CurTime)
        {
            component.Announced = true;

            if (!string.IsNullOrEmpty(component.Announcement))
                _chat.DispatchGlobalAnnouncement(component.Announcement, component.Sender, colorOverride: component.AnnouncementColor);
        }

        CheckRoundEnd(uid, component, gameRule);
    }

    private void CheckRoundEnd(EntityUid uid, XenomorphsRuleComponent component, GameRuleComponent gameRule)
    {
        var stationGrids = GetStationGrids();

        var humans = GetHumans(stationGrids);
        var xenomorphs = GetXenomorphs(component);

        if (xenomorphs.Count == 0)
        {
            if (component.Announced && !string.IsNullOrEmpty(component.NoMoreThreatAnnouncement))
                _chat.DispatchGlobalAnnouncement(component.NoMoreThreatAnnouncement, component.Sender, colorOverride: component.NoMoreThreatAnnouncementColor);

            component.WinType = WinType.CrewMajor;
            component.WinConditions.Add(WinCondition.AllReproduceXenoDead);
            ForceEndSelf(uid, gameRule);
        }

        if (xenomorphs.Count / (float) (xenomorphs.Count + GetHumans(stationGrids, true).Count) >= 1)
        {
            component.WinType = WinType.XenoMajor;
            component.WinConditions.Add(WinCondition.AllCrewDead);
            ForceEndSelf(uid, gameRule);
            _roundEnd.EndRound();
            return;
        }

        if (!component.Announced || component.WinType == WinType.XenoMinor
            || xenomorphs.Count / (float) (xenomorphs.Count + humans.Count) < component.XenomorphsShuttleCallPercentage)
            return;

        _roundEnd.DoRoundEndBehavior(
            RoundEndBehavior.ShuttleCall,
            component.ShuttleCallTime,
            component.RoundEndTextSender,
            component.RoundEndTextShuttleCall,
            component.RoundEndTextAnnouncement
        );

        component.WinType = WinType.XenoMinor;
        component.WinConditions.Add(WinCondition.XenoTakeoverStation);

        var station = _station.GetStations().FirstOrNull();
        if (!station.HasValue)
            return;

        _nukeCodePaper.SendNukeCodes(station.Value);
    }

    private List<EntityUid> GetHumans(HashSet<EntityUid>? stationGrids = null, bool includeOffStation = false)
    {
        var humans = new List<EntityUid>();
        stationGrids ??= GetStationGrids();

        var players = AllEntityQuery<HumanoidAppearanceComponent, ActorComponent, MobStateComponent, TransformComponent>();
        while (players.MoveNext(out var uid, out _, out _, out var mobStateComponent, out var xform))
        {
            if (_mobState.IsDead(uid, mobStateComponent)
                || !includeOffStation && !stationGrids.Contains(xform.GridUid ?? EntityUid.Invalid))
                continue;

            humans.Add(uid);
        }

        return humans;
    }

    private List<EntityUid> GetXenomorphs(XenomorphsRuleComponent xenomorphsRule, ProtoId<XenomorphCastePrototype>? cast = null)
    {
        var xenomorphs = new List<EntityUid>();

        foreach(var xenomorph in xenomorphsRule.Xenomorphs.ToList())
        {
            if (!Exists(xenomorph) || !TryComp<XenomorphComponent>(xenomorph, out var xenomorphComponent))
            {
                xenomorphsRule.Xenomorphs.Remove(xenomorph);
                continue;
            }

            if (_mobState.IsDead(xenomorph) || cast.HasValue && xenomorphComponent.Caste != cast)
                continue;

            xenomorphs.Add(xenomorph);
        }

        return xenomorphs;
    }

    private HashSet<EntityUid> GetStationGrids()
    {
        var stationGrids = new HashSet<EntityUid>();
        foreach (var station in _gameTicker.GetSpawnableStations())
        {
            if (TryComp<StationDataComponent>(station, out var data) && _station.GetLargestGrid(data) is { } grid)
                stationGrids.Add(grid);
        }

        return stationGrids;
    }
}
