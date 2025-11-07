using Content.Goobstation.Server.Werewolf.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles
;using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Werewolf.Systems;

public sealed class WerewolfRuleSystem : GameRuleSystem<WerewolfRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public readonly SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/changeling_start.ogg"); // todo

    public readonly ProtoId<AntagPrototype> WerewolfPrototypeId = "Werewolf";

    public readonly ProtoId<CurrencyPrototype> Currency = "Fury";

    public readonly int StartingCurrency = 1;

    [ValidatePrototypeId<EntityPrototype>] EntProtoId mindRole = "MindRoleWerewolf";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        // SubscribeLocalEvent<WerewolfRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnSelectAntag(EntityUid uid, WerewolfRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeWerewolf(args.EntityUid, comp);
    }
    public bool MakeWerewolf(EntityUid target, WerewolfRuleComponent rule)
    {
        if (HasComp<SiliconComponent>(target))
            return false;

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, mindRole.Id, mind, true);

        var briefing = Loc.GetString("werewolf-role-greeting")
        ;var briefingShort = Loc.GetString("werewolf-role-greeting-short")
        ;_antag.SendBriefing(target, briefing, Color.Brown, BriefingSound);
        if (_role.MindHasRole<WerewolfRuleComponent>(mindId, out var mr))
                AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefingShort }, overwrite: true);

        EnsureComp<WerewolfBasicAbilitiesComponent>(target); // todo shite

        EnsureComp<WerewolfMindComponent>(mindId);

        // add store
        var store = EnsureComp<StoreComponent>(target);
        foreach (var category in rule.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(Currency);
        store.Balance.Add(Currency, StartingCurrency);

        rule.WerewolfMinds.Add(mindId);

        return true;
    }

    // todo OnTextPrepend
}
