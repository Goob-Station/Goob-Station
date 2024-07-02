using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.Changeling;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store.Components;

namespace Content.Server.GameTicking.Rules;

public sealed partial class ChangelingRuleSystem : GameRuleSystem<ChangelingRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    public void OnSelectAntag(EntityUid uid, ChangelingRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeChangeling(args.EntityUid, comp);
    }

    public bool MakeChangeling(EntityUid target, ChangelingRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        TryComp<MetaDataComponent>(target, out var metaData);

        var briefing = Loc.GetString("changeling-role-greeting", ("name", metaData?.EntityName ?? "Unknown"));
        var briefingShort = Loc.GetString("changeling-role-greeting-short", ("name", metaData?.EntityName ?? "Unknown"));

        _antag.SendBriefing(target, briefing, Color.Yellow, rule.BriefingSound);

        _role.MindAddRole(mindId, new RoleBriefingComponent { Briefing = briefingShort }, mind, true);

        _npcFaction.RemoveFaction(target, rule.NanotrasenFactionId, false);
        _npcFaction.AddFaction(target, rule.ChangelingFactionId);

        EnsureComp<ChangelingComponent>(target);

        foreach (var actionId in rule.BaseChangelingActions)
            _actions.AddAction(target, actionId);

        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(rule.Currency);
        store.Balance.Add(rule.Currency, 20);

        rule.ChangelingMinds.Add(mindId);

        return true;
    }
}
