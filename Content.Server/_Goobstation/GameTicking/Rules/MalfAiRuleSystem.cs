using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.MalfAi;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Server.GameTicking.Rules;

public sealed partial class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/Ambience/Antag/emagged_borg.ogg");
    public readonly ProtoId<AntagPrototype> MalfAiPrototypeId = "MalfAi";
    public readonly ProtoId<CurrencyPrototype> Currency = "ControlPower";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }
    private void OnSelectAntag(EntityUid uid, MalfAiRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeMalfAi(args.EntityUid, comp);
    }
    public bool MakeMalfAi(EntityUid target, MalfAiRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;


        if (TryComp<MetaDataComponent>(target, out var metaData))
        {
            var briefing = Loc.GetString("malfai-role-greeting", ("name", metaData?.EntityName ?? "Unknown"));
            var briefingShort = Loc.GetString("malfai-role-greeting-short", ("name", metaData?.EntityName ?? "Unknown"));

            _antag.SendBriefing(target, briefing, Color.Green, BriefingSound);
            _role.MindAddRole(mindId, new RoleBriefingComponent { Briefing = briefingShort }, mind, true);
        }
        EnsureComp<MalfAiComponent>(target);
        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(Currency);
        store.Balance.Add(Currency, 16);

        rule.MalfAiMind.Add(mindId);

        foreach (var objective in rule.Objectives)
            _mind.TryAddObjective(mindId, mind, objective);

        return true;
    }

}
