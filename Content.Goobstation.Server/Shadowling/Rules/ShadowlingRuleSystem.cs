using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Zombies;
using Content.Shared._EE.Shadowling;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NpcFactionSystem _npc = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/shadowling.ogg");

    [ValidatePrototypeId<EntityPrototype>] EntProtoId _mindRole = "MindRoleShadowling";

    public readonly ProtoId<NpcFactionPrototype> ShadowlingFactionId = "Shadowling";

    public readonly ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<ShadowlingRoleComponent, GetBriefingEvent>(OnGetBriefing);

        SubscribeLocalEvent<ShadowlingAscendEvent>(OnAscend);
        SubscribeLocalEvent<ShadowlingDeathEvent>(OnDeath);
    }

    private void OnDeath(ShadowlingDeathEvent args)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var shadowling, out _))
        {
            var shadowlingCount = 0;
            var shadowlingDead = 0;
            var query = EntityQueryEnumerator<ShadowlingComponent>();

            while (query.MoveNext(out var uid, out _))
            {
                shadowlingCount++;
                if (_mob.IsDead(uid) || _mob.IsInvalidState(uid))
                    shadowlingDead++;
            }

            if (shadowlingCount == shadowlingDead)
                shadowling.WinCondition = ShadowlingWinCondition.Failure;
        }
    }

    private void OnAscend(ShadowlingAscendEvent args)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var shadowling, out _))
        {
            shadowling.WinCondition = ShadowlingWinCondition.Win;
            return;
        }
    }

    private void OnGetBriefing(EntityUid uid, ShadowlingRoleComponent component, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;
        var sling = HasComp<ShadowlingComponent>(ent);
        args.Briefing = Loc.GetString(sling ? "shadowling-briefing" : "thrall-briefing");
    }

    private void OnSelectAntag(EntityUid uid, ShadowlingRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeShadowling(args.EntityUid, comp);
    }

    public bool MakeShadowling(EntityUid target, ShadowlingRuleComponent rule)
    {
        EnsureComp<ZombieImmuneComponent>(target);

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, _mindRole.Id, mind, true);

        _npc.RemoveFaction(target, NanotrasenFactionId, false);
        _npc.AddFaction(target, ShadowlingFactionId);

        TryComp<MetaDataComponent>(target, out var metaData);
        if (metaData == null)
            return false;

        var briefing = Loc.GetString("shadowling-role-greeting");

        _antag.SendBriefing(target, briefing, Color.MediumPurple, BriefingSound);

        EnsureComp<ShadowlingComponent>(target);

        rule.ShadowlingMinds.Add(mindId);
        return true;
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        ShadowlingRuleComponent component,
        GameRuleComponent gamerule,
        ref RoundEndTextAppendEvent args
    )
    {
        base.AppendRoundEndText(uid, component, gamerule, ref args);
        var winText = Loc.GetString($"shadowling-condition-{component.WinCondition.ToString().ToLower()}");
        args.AddLine(winText);

        args.AddLine(Loc.GetString("shadowling-list-start"));

        var sessionData = _antag.GetAntagIdentifiers(uid);
        foreach (var (_, data, name) in sessionData)
        {
            var listing = Loc.GetString("shadowling-list-name", ("name", name), ("user", data.UserName));
            args.AddLine(listing);
        }
    }
}
