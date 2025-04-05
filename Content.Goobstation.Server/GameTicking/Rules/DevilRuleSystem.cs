using Content.Goobstation.Server.Devil;
using Content.Goobstation.Server.Devil.Roles;
using Content.Goobstation.Shared.Devil;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.GameTicking.Rules;

public sealed class DevilRuleSystem : GameRuleSystem<DevilRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;

    private readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/changeling_start.ogg");

    [ValidatePrototypeId<NpcFactionPrototype>]
    public const string DevilFaction = "DevilFaction";

    public readonly ProtoId<AntagPrototype> DevilPrototypeId = "Devil";

    private EntProtoId DevilMindRole = "DevilMindRole";

    [ValidatePrototypeId<CurrencyPrototype>]
    public const string SoulCurrency = "SoulPoint";

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string NanotrasenFaction = "NanoTrasen";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    private void OnSelectAntag(EntityUid uid, DevilRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeDevil(args.EntityUid, comp);
    }

    private bool MakeDevil(EntityUid target, DevilRuleComponent rule)
    {

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, DevilMindRole.Id, mind, true);

        var devilComp = EnsureComp<DevilComponent>(target);

        var briefing = Loc.GetString("devil-role-greeting", ("trueName", devilComp.TrueName));

        _antag.SendBriefing(target, briefing, Color.Yellow, BriefingSound);

        if (_role.MindHasRole<DevilRoleComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefing }, overwrite: true);

        _npcFaction.RemoveFaction(target, NanotrasenFaction);
        _npcFaction.AddFaction(target, DevilFaction);

        EnsureComp<DevilComponent>(target);

        var store = EnsureComp<StoreComponent>(target);

        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);

        store.CurrencyWhitelist.Add(SoulCurrency);

        rule.DevilMinds.Add(mindId);

        return true;
    }
}
