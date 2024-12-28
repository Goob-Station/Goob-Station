using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server._Goobstation.Objectives.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Shared.Mindflayer;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Text;

namespace Content.Server.GameTicking.Rules;

public sealed partial class MindflayerRuleSystem : GameRuleSystem<MindflayerRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Mindflayer/Ambience/Antag/Mindflayer/sound_ambience_antag_mindflayer_alert.ogg");

    [ValidatePrototypeId<NpcFactionPrototype>] public readonly ProtoId<NpcFactionPrototype> MindflayerFactionId = "Mindflayer";

    [ValidatePrototypeId<NpcFactionPrototype>] public readonly ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

    [ValidatePrototypeId<CurrencyPrototype>] public readonly ProtoId<CurrencyPrototype> Currency = "Swarms";

    [ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleMindflayer";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindflayerRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
        //SubscribeLocalEvent<MindflayerRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnAntagSelect(Entity<MindflayerRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {

        TryMakeMindflayer(args.EntityUid, ent.Comp);

    }

    public bool TryMakeMindflayer(EntityUid target, MindflayerRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, mindRole.Id, mind, true);

        // briefing
        if (HasComp<MetaDataComponent>(target))
        {
            var briefingShort = Loc.GetString("mindflayer-role-greeting-short");

            _antag.SendBriefing(target, Loc.GetString("mindflayer-role-greeting-fluff"), Color.MediumPurple, null);
            _antag.SendBriefing(target, Loc.GetString("mindflayer-role-greeting"), Color.Red, BriefingSound);

            if (_role.MindHasRole<MindflayerRoleComponent>(mindId, out var mr))
                AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefingShort }, overwrite: true);
        }
        _npcFaction.RemoveFaction(target, NanotrasenFactionId, false);
        _npcFaction.AddFaction(target, MindflayerFactionId);

        EnsureComp<MindflayerComponent>(target);

        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(Currency);
        store.Balance.Add(Currency, 2);

        rule.Minds.Add(mindId);

        return true;
    }
    /*
    public void OnTextPrepend(Entity<HereticRuleComponent> ent, ref ObjectivesTextPrependEvent args)
    {
        var sb = new StringBuilder();

        var mostKnowledge = 0f;
        var mostKnowledgeName = string.Empty;

        foreach (var heretic in EntityQuery<HereticComponent>())
        {
            if (!_mind.TryGetMind(heretic.Owner, out var mindId, out var mind))
                continue;

            var name = _objective.GetTitle((mindId, mind), Name(heretic.Owner));
            if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
            {
                if (objective.Researched > mostKnowledge)
                    mostKnowledge = objective.Researched;
                mostKnowledgeName = name;
            }

            var str = Loc.GetString($"roundend-prepend-heretic-ascension-{(heretic.Ascended ? "success" : "fail")}", ("name", name));
            sb.AppendLine(str);
        }

        sb.AppendLine("\n" + Loc.GetString("roundend-prepend-heretic-knowledge-named", ("name", mostKnowledgeName), ("number", mostKnowledge)));

        args.Text = sb.ToString();
    }
    */
}
