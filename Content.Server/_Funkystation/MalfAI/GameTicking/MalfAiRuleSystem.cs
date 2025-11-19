// SPDX-FileCopyrightText: 2025 TyraFox <
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Alert;
using Robust.Server.Player;
using Content.Server.Actions;
using Content.Server.Silicons.Laws;
using Content.Shared.Mind;
using Content.Server.Objectives;
using Robust.Shared.Random;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Store.Systems;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Store.Components;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking;

namespace Content.Server._Funkystation.GameTicking;

/// <summary>
/// Handles Malf AI rule startup behavior. When the rule is added mid-round via admin command,
/// immediately assigns the currently active Station AI as the Malf AI.
/// </summary>
public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaws = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private readonly ISawmill _sawmill = Logger.GetSawmill("malfai");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagEntitySelected);
        SubscribeLocalEvent<MalfAiRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
    }

    private void SeedDefaultLaws(EntityUid rule)
    {
        // Ensure master lawset storage & law container exist on the rule entity.
        var holder = EnsureComp<MalfMasterLawsetComponent>(rule);
        EnsureComp<SiliconLawBoundComponent>(rule);
        EnsureComp<SiliconLawProviderComponent>(rule);

        // If empty, seed with defaults so the editor shows contents on first open.
        if (holder.Laws.Count == 0)
        {
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-1"));
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-2"));
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-3"));
        }

        // Initialize the SiliconLawBoundComponent with the laws from MalfMasterLawsetComponent
        // so the law editor can properly read and display them
        var defaultLaws = holder.Laws.Select((law, index) => new SiliconLaw
        {
            LawString = law,
            Order = index + 1
        }).ToList();

        _siliconLaws.SetLaws(defaultLaws, rule);
    }

    private void OnAfterAntagEntitySelected(Entity<MalfAiRuleComponent> rule, ref AfterAntagEntitySelectedEvent args)
    {
        // Apply malf AI setup to the selected antagonist entity.
        ApplyMalfSetup(args.EntityUid);
    }

    private void ApplyMalfSetup(Entity<MalfunctioningAiComponent?> malf)
    {
        malf.Comp = EnsureComp<MalfunctioningAiComponent>(malf.Owner);

        var store = EnsureComp<StoreComponent>(malf.Owner);
        store.Name = malf.Comp.StoreName;

        foreach (var category in malf.Comp.StoreCategories)
        {
            if (!store.Categories.Contains(category))
                store.Categories.Add(category);
        }

        store.CurrencyWhitelist.Add(malf.Comp.CurrencyId);

        _store.TryAddCurrency(new() { { malf.Comp.CurrencyId, FixedPoint2.New(0) } }, malf.Owner, store);

        _actions.AddAction(malf.Owner, malf.Comp.OpenStoreAction);
        _actions.AddAction(malf.Owner, malf.Comp.OpenBorgsUiAction);

        EnsureComp<MalfAiCameraUpgradeComponent>(malf.Owner);

        // Ensure AlertsComponent exists before showing alert and show CPU alert HUD on the client
        EnsureComp<AlertsComponent>(malf.Owner);
        _alerts.ShowAlert(malf.Owner, malf.Comp.CurrencyAlertId);
    }

    private bool TryPickUniqueAssassinationTarget(EntityUid aiEnt, HashSet<EntityUid> reserved, [NotNullWhen(true)] out EntityUid picked)
    {
        picked = default;

        var query = EntityQueryEnumerator<MindComponent>();
        var candidates = new HashSet<Entity<MindComponent>>();

        while (query.MoveNext(out var uid, out var mind))
        {
            if (uid == aiEnt)
                continue;

            if (HasComp<MalfunctioningAiComponent>(uid))
                continue;

            candidates.Add((uid, mind));
        }

        if (!candidates.Any())
            return false;

        picked = _random.Pick(candidates);
        return true;
    }

    protected override void Started(EntityUid uid, MalfAiRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        SeedDefaultLaws(uid);
    }

    protected override void Added(EntityUid uid, MalfAiRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        // Preselect (pending) the current Station AI, if any, respecting preferences and whitelist.
        // This does not force assignment; actual assignment happens per AntagSelection timing.
        if (!TryComp<AntagSelectionComponent>(uid, out var ruleAntagComp))
            return;

        var ruleAntagEnt = new Entity<AntagSelectionComponent>(uid, ruleAntagComp);
        var def = ruleAntagComp.Definitions.FirstOrDefault();

        if (def.Equals(default(AntagSelectionDefinition)))
            return;

        var session = _players.Sessions.FirstOrDefault(session =>
            session.AttachedEntity is not null
            && HasComp<StationAiHeldComponent>(session.AttachedEntity));

        if (session?.AttachedEntity == null)
        {
            _sawmill.Warning("[MalfAI] No valid session found for MalfAi.");
            return;
        }

        // If we are mid-round (i.e., game rule was added after round started), assign immediately.
        // Otherwise, just preselect and let the normal selection flow handle it.
        var isMidRound = _ticker.RunLevel == GameRunLevel.InRound;

        _sawmill.Debug($"[MalfAI] {(isMidRound ? "Assigning" : "Preselecting")} {session.Name} as Malf AI.");
        _antag.TryMakeAntag(ruleAntagEnt, session, def, ignoreSpawner: true, checkPref: true, onlyPreSelect: !isMidRound);
    }

    private void OnObjectivesTextGetInfo(Entity<MalfAiRuleComponent> rule, ref ObjectivesTextGetInfoEvent args)
    {
        args.AgentName = Loc.GetString("malfai-round-end-result");

        var antags = _antag.GetAntagIdentifiers(rule.Owner);

        foreach (var (mindId, _, name) in antags)
            args.Minds.Add((mindId, name));
    }
}
