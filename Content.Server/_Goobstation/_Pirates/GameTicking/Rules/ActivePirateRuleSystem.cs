using Content.Server._Goobstation._Pirates.Pirates.Siphon;
using Content.Server._Goobstation._Pirates.Roles;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.GameTicking.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation._Pirates.GameTicking.Rules;

public sealed partial class ActivePirateRuleSystem : GameRuleSystem<ActivePirateRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;

    private static readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/Ambience/Antag/pirate_start.ogg");
    private static readonly EntProtoId MindRole = "MindRolePirate";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActivePirateRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
        SubscribeLocalEvent<PirateRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnAntagSelect(Entity<ActivePirateRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (_mind.TryGetMind(args.EntityUid, out var mindId, out var mind) && TryMakePirate(args.EntityUid))
            ent.Comp.Pirates.Add((mindId, mind));
    }

    private void OnGetBriefing(Entity<PirateRoleComponent> ent, ref GetBriefingEvent args)
    {
        var briefingShort = Loc.GetString("antag-pirate-briefing-short");
        args.Briefing = briefingShort;
    }

    protected override void AppendRoundEndText(EntityUid uid, ActivePirateRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        if (component.BoundSiphon != null
        && TryComp<ResourceSiphonComponent>(component.BoundSiphon, out var siphon)
        && siphon.Active)
            args.AddLine(Loc.GetString("pirate-roundend-append-siphon", ("num", siphon.Credits)));

        args.AddLine(Loc.GetString("pirate-roundend-append", ("num", component.Credits)));

        args.AddLine($"\n{Loc.GetString("pirate-roundend-list")}");
        var antags = _antag.GetAntagIdentifiers(uid);
        foreach (var (_, sessionData, name) in antags)
        {
            // nukies? in my pirate rule? how queer...
            args.AddLine(Loc.GetString("nukeops-list-name-user", ("name", name), ("user", sessionData.UserName)));
        }
    }

    public bool TryMakePirate(EntityUid target)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, MindRole.Id, mind, true);

        var briefing = Loc.GetString("antag-pirate-briefing");
        _antag.SendBriefing(target, briefing, Color.OrangeRed, BriefingSound);

        _npcFaction.AddFaction(target, "PirateFaction"); // yaml fucking sucks!!!

        return true;
    }
}
