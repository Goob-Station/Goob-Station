using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared.GameTicking.Components;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Server.Cult.GameTicking;
public sealed partial class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IVoteManager _votes = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    public static readonly SoundSpecifier GainSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_gain.ogg");

    public static readonly SoundSpecifier UnimportantAnnouncementSound = new SoundCollectionSpecifier("bloodCrawl");
    public static readonly ProtoId<LanguagePrototype> CultLanguage = "Eldritch";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagEntitySelected);
    }

    protected override void ActiveTick(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        // election countdown exists and is over
        var election = component.LeaderElectionCountdown;
        if (election.HasValue && _timing.CurTime >= election)
        {
            component.LeaderElectionCountdown = null;
            BeginLeaderElection((uid, component));
        }

        // tier change countdown exists and is over
        var tierChange = component.TierChangeCountdown;
        if (tierChange.HasValue && _timing.CurTime >= tierChange.Value)
        {
            component.TierChangeCountdown = null;
            UpdateTier((uid, component), component.ScheduledTier);
        }

        // if there's still a scheduled tier change but the countdown is null, schedule it again
        if (component.ScheduledTier.HasValue && !tierChange.HasValue)
            ScheduleTierUpdate((uid, component), component.ScheduledTier.Value);
    }

    public bool TryGetRule(out Entity<BloodCultRuleComponent>? ent)
    {
        ent = null;
        var q = EntityQueryEnumerator<ActiveGameRuleComponent, BloodCultRuleComponent>();
        while (q.MoveNext(out var uid, out var active, out var rule))
        {
            ent = new Entity<BloodCultRuleComponent>(uid, rule);
            break; // == first one selected
        }

        return ent.HasValue;
    }

    public void DoCultAnnouncement(Entity<BloodCultRuleComponent> ent, string message, SoundSpecifier? sound = null, int size = 18)
    {
        if (sound == null) sound = UnimportantAnnouncementSound;
        foreach (var cultist in ent.Comp.Cultists)
            _antag.SendBriefing(cultist, $"[size={size}]{message}[/size]", Color.Crimson, sound);
    }

    #region Leadership

    public void MakeCultLeader(Entity<BloodCultRuleComponent> ent, EntityUid target)
    {
        if (ent.Comp.CultLeader.HasValue)
            RemComp<BloodCultistLeaderComponent>(ent.Comp.CultLeader.Value);

        EnsureComp<BloodCultistLeaderComponent>(target);
        ent.Comp.CultLeader = target;
    }

    public void ScheduleLeaderElection(Entity<BloodCultRuleComponent> ent)
    {
        if (ent.Comp.LeaderElectionCountdown.HasValue) // scheduled already
            return;

        // only one dude remaining? or starting with? whatever bro make him a cultie.
        if (ent.Comp.Cultists.Count == 1)
        {
            MakeCultLeader(ent, ent.Comp.Cultists.First());
            return;
        }

        ent.Comp.LeaderElectionCountdown = _timing.CurTime + ent.Comp.LeaderElectionTimer;
    }

    public void BeginLeaderElection(Entity<BloodCultRuleComponent> ent)
    {
        var cultists = new List<(string, EntityUid)>();

        var cultQuery = EntityQueryEnumerator<BloodCultistComponent, MetaDataComponent>();
        while (cultQuery.MoveNext(out var cult, out var cultComp, out var metadata))
        {
            var playerInfo = metadata.EntityName;
            cultists.Add((playerInfo, cult));
        }

        var options = new VoteOptions
        {
            DisplayVotes = false,
            Title = Loc.GetString("cult-vote-master-title"),
            InitiatorText = Loc.GetString("cult-vote-master-initiator"),
            Duration = ent.Comp.LeaderElectionTimer,
            VoterEligibility = VoteManager.VoterEligibility.BloodCult
        };

        foreach (var (name, uid) in cultists)
            options.Options.Add((Loc.GetString(name), uid));

        var vote = _votes.CreateVote(options);

        vote.OnFinished += (_, args) =>
        {
            EntityUid picked;

            if (args.Winner == null) picked = (EntityUid) _rand.Pick(args.Winners);
            else picked = (EntityUid) args.Winner;
            MakeCultLeader(ent, picked);
        };
    }

    #endregion

    #region Tiers

    public void UpdateTierBasedOnCultistCount(Entity<BloodCultRuleComponent> ent)
    {
        var cultists = ent.Comp.Cultists.Count;
        var players = _antag.GetAliveConnectedPlayers(_player.Sessions);
        var ratio = (float) cultists / players.Count;

        // this could've been done directly
        // but in case there's 70% of players converted and somehow the tier is still None
        // it would get freaky
        BloodCultTier? scheduledTier = null;
#if DEBUG
        foreach (var tier in ent.Comp.DebugCultistsTierRatio)
            if (cultists >= tier.Key && (int) ent.Comp.CurrentTier < (int) tier.Value)
                scheduledTier = tier.Value;
#else
        foreach (var tier in ent.Comp.TierPercentageRatio)
            if (ratio >= tier.Key && (int) ent.Comp.CurrentTier < (int) tier.Value)
                scheduledTier = tier.Value;
#endif
        if (scheduledTier.HasValue)
            ScheduleTierUpdate(ent, scheduledTier.Value);

    }

    public void ScheduleTierUpdate(Entity<BloodCultRuleComponent> ent, BloodCultTier newTier)
    {
        if (ent.Comp.TierChangeCountdown.HasValue) // scheduled already
            return;

        ent.Comp.ScheduledTier = newTier;
        ent.Comp.TierChangeCountdown = _timing.CurTime + ent.Comp.TierChangeTimer;

        if (!ent.Comp.TierData.TryGetValue(newTier, out var data))
            return;

        DoCultAnnouncement(ent, Loc.GetString(data.Announcement), data.Sound);
    }

    public void UpdateTier(Entity<BloodCultRuleComponent> ent, BloodCultTier? newTier)
    {
        if (!newTier.HasValue) return;

        ent.Comp.ScheduledTier = null;
        ent.Comp.CurrentTier = newTier.Value;
        foreach (var cultist in ent.Comp.Cultists)
            ApplyTierEffects(cultist, newTier.Value);
    }

    public void ApplyTierEffects(EntityUid ent, BloodCultTier tier)
    {
        if (tier >= BloodCultTier.Eyes)
            EnsureComp<BloodCultVisualEyesComponent>(ent);

        if (tier >= BloodCultTier.Halos)
            EnsureComp<BloodCultVisualHaloComponent>(ent);
    }

    #endregion

    protected override void Added(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryGetRule(out var existing))
            return;

        // mutex check - end that new game rule in favor of the last one.
        // add unique cultists from there
        existing!.Value.Comp.Cultists.AddRange(component.Cultists.Where(cultist => !component.Cultists.Contains(cultist)));

        _ticker.EndGameRule(uid, gameRule);
    }

    private void OnAfterAntagEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeCultist(args.EntityUid, ent, roundstart: true);
    }

    public void MakeCultist(EntityUid target, Entity<BloodCultRuleComponent> rule, bool roundstart = false)
    {
        if (HasComp<BloodCultistComponent>(target)) // already?
            return;

        EnsureComp<BloodCultistComponent>(target);
        EnsureComp<BloodMagicProviderComponent>(target);

        var gain = roundstart ? Loc.GetString("cult-gain-briefing") : Loc.GetString("cult-gain-convert");
        _antag.SendBriefing(target, Loc.GetString("cult-gain-bloat"), Color.Crimson, null);
        _antag.SendBriefing(target, gain, Color.Red, GainSound);

        if (!rule.Comp.CultLeader.HasValue)
            ScheduleLeaderElection(rule);
    }
}
