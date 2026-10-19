// SPDX-FileCopyrightText: 2025 TyraFox <
// SPDX-License-Identifier: MIT

using System.Linq;
using Robust.Server.GameObjects;
using Content.Shared.Store;
using Content.Shared._Goobstation.Wizard.Refund;
using Content.Shared.MalfAI;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Alert;
using Robust.Server.Player;
using Content.Shared.Roles;
using Content.Server.Mind;
using Content.Server.Actions;
using Content.Server.Silicons.Laws;
using Content.Shared.Mind;
using Content.Server.Objectives;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Handles Malf AI rule startup behavior. When the rule is added mid-round via admin command,
/// immediately assigns the currently active Station AI as the Malf AI.
/// </summary>
public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly Store.Systems.StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaws = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedRoleSystem _sharedRoleSystem = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
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
        var holder = EnsureComp<Content.Shared.MalfAI.MalfMasterLawsetComponent>(rule);
        EnsureComp<Content.Shared.Silicons.Laws.Components.SiliconLawBoundComponent>(rule);
        EnsureComp<Content.Shared.Silicons.Laws.Components.SiliconLawProviderComponent>(rule);

        // If empty, seed with defaults so the editor shows contents on first open.
        if (holder.Laws.Count == 0)
        {
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-1"));
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-2"));
            holder.Laws.Add(Loc.GetString("silicon-law-malfai-master-3"));
        }

        // Initialize the SiliconLawBoundComponent with the laws from MalfMasterLawsetComponent
        // so the law editor can properly read and display them
        var defaultLaws = holder.Laws.Select((law, index) => new Content.Shared.Silicons.Laws.SiliconLaw
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

    private void ApplyMalfSetup(EntityUid aiEnt)
    {
        // Mark as Malf AI for special interactions
        EnsureComp<Content.Shared.MalfAI.MalfAiMarkerComponent>(aiEnt);

        // Add the required user interfaces so actions can open them
        EnsureComp<UserInterfaceComponent>(aiEnt);
        _ui.SetUi(aiEnt, StoreUiKey.Key, new InterfaceData("StoreBoundUserInterface", -1, false));
        _ui.SetUi(aiEnt, RefundUiKey.Key, new InterfaceData("StoreRefundBoundUserInterface", -1, false));
        _ui.SetUi(aiEnt, MalfAiBorgsUiKey.Key, new InterfaceData("MalfAiBorgsBoundUserInterface", -1, false));

        var store = EnsureComp<Content.Shared.Store.Components.StoreComponent>(aiEnt);
        // Configure store minimal settings
        store.Name = "store-preset-name-malfai";

        var requiredCategories = new[] { "Deception", "Factory", "Disruption" };
        foreach (var category in requiredCategories)
        {
            if (!store.Categories.Contains(category))
                store.Categories.Add(category);
        }

        if (!store.CurrencyWhitelist.Contains("CPU"))
            store.CurrencyWhitelist.Add("CPU");

        // Ensure UI updates and set starting balance to 0 CPU
        _store.TryAddCurrency(new() { { "CPU", Content.Goobstation.Maths.FixedPoint.FixedPoint2.New(0) } }, aiEnt, store);

        // Grant the Open Shop action to the AI entity
        _actions.AddAction(aiEnt, "ActionMalfAiOpenStore");
        // Grant the Borgs management UI action
        _actions.AddAction(aiEnt, "ActionMalfAiOpenBorgsUi");

        EnsureComp<Content.Shared.MalfAI.MalfAiCameraUpgradeComponent>(aiEnt);

        // Ensure AlertsComponent exists before showing alert and show CPU alert HUD on the client
        EnsureComp<AlertsComponent>(aiEnt);
        _alerts.ShowAlert(aiEnt, "MalfCpu");
    }





    private bool TryPickUniqueAssassinationTarget(EntityUid aiEnt, HashSet<EntityUid> reserved, out EntityUid picked)
    {
        picked = default;

        // Eligible: crew with a mind, not the AI, and not another Malf AI.
        var candidates = EntityQuery<MindComponent>()
            .Where(m => m.Owner != aiEnt &&
                       !HasComp<Content.Shared.MalfAI.MalfAiMarkerComponent>(m.Owner) &&
                       !reserved.Contains(m.Owner))
            .Select(m => m.Owner)
            .ToList();

        if (candidates.Count == 0)
            return false;

        // Use direct random selection since candidates are already filtered
        picked = _random.Pick(candidates);
        return true;
    }

    protected override void Started(EntityUid uid, MalfAiRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Seed the default master lawset on the rule entity so it is available in editors/logic.
        SeedDefaultLaws(uid);
    }

    protected override void Added(EntityUid uid, MalfAiRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        // Preselect (pending) the current Station AI, if any, respecting preferences and whitelist.
        // This does not force assignment; actual assignment happens per AntagSelection timing.
        if (TryComp<AntagSelectionComponent>(uid, out var ruleAntagComp))
        {
            var ruleAntagEnt = new Entity<AntagSelectionComponent>(uid, ruleAntagComp);
            var def = ruleAntagComp.Definitions.FirstOrDefault();
            if (!def.Equals(default(AntagSelectionDefinition)))
            {
                var session = _players.Sessions.FirstOrDefault(s =>
                    s.AttachedEntity != null && HasComp<Content.Shared.Silicons.StationAi.StationAiCustomizationComponent>(s.AttachedEntity));
                if (session?.AttachedEntity == null)
                {
                    _sawmill.Warning("[MalfAI] No valid session found for MalfAi.");
                    return;
                }

                // If we are mid-round (i.e., game rule was added after round started), assign immediately.
                // Otherwise, just preselect and let the normal selection flow handle it.
                var ticker = EntitySystem.Get<GameTicker>();
                var isMidRound = ticker.RunLevel == GameRunLevel.InRound;

                _sawmill.Debug($"[MalfAI] {(isMidRound ? "Assigning" : "Preselecting")} {session.Name} as Malf AI.");
                _antag.TryMakeAntag(ruleAntagEnt, session, def, ignoreSpawner: true, checkPref: true, onlyPreSelect: !isMidRound);
            }
        }
    }

    private void OnObjectivesTextGetInfo(EntityUid uid, MalfAiRuleComponent component, ref ObjectivesTextGetInfoEvent args)
    {
        args.AgentName = Loc.GetString("malfai-round-end-result");

        var antags = _antag.GetAntagIdentifiers(uid);
        foreach (var (mindId, _, name) in antags)
        {
            args.Minds.Add((mindId, name));
        }
    }

}
