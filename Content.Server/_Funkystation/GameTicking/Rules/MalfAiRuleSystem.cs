// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server._Funkystation.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server._Funkystation.MalfAI;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Silicons.Laws;
using Content.Server.Store.Systems;
using Content.Shared.Alert;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Roles;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.GameTicking.Rules;

public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<CurrencyPrototype> CpuCurrency = "CPU";

    private static readonly ProtoId<StoreCategoryPrototype> CategoryMalfAI = "MalfAI";
    private static readonly ProtoId<StoreCategoryPrototype> CategoryDeception = "MalfAIDeception";
    private static readonly ProtoId<StoreCategoryPrototype> CategoryFactory = "MalfAIFactory";
    private static readonly ProtoId<StoreCategoryPrototype> CategoryDisruption = "MalfAIDisruption";

    private static readonly ProtoId<AlertPrototype> CpuAlert = "MalfCpu";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagEntitySelected);
        SubscribeLocalEvent<MalfAiMarkerComponent, OpenMalfAiStoreActionEvent>(OnOpenStore);
    }

    private void OnAfterAntagEntitySelected(Entity<MalfAiRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        ApplyMalfSetup(args.EntityUid);
    }

    private void OnOpenStore(Entity<MalfAiMarkerComponent> ent, ref OpenMalfAiStoreActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StoreComponent>(ent.Owner, out var store))
            return;

        _store.ToggleUi(ent.Owner, ent.Owner, store);
        args.Handled = true;
    }

    public void ApplyMalfSetup(EntityUid aiEntity)
    {
        // Mark as Malf AI
        EnsureComp<MalfAiMarkerComponent>(aiEntity);

        // Set up the master lawset holder
        var masterLawset = EnsureComp<MalfMasterLawsetComponent>(aiEntity);
        SeedDefaultMasterLaws(masterLawset);

        // Override the AI's own laws with malf laws
        SeedAiLaws(aiEntity);

        // Add the CPU upgrade store
        var store = EnsureComp<StoreComponent>(aiEntity);
        store.Categories.Add(CategoryMalfAI);
        store.Categories.Add(CategoryDeception);
        store.Categories.Add(CategoryFactory);
        store.Categories.Add(CategoryDisruption);
        store.CurrencyWhitelist.Add(CpuCurrency);
        store.Balance[CpuCurrency] = 0;
        store.Name = "malfai-store-title";

        // Grant the store-opening action
        _actions.AddAction(aiEntity, "ActionMalfAiOpenStore");
        // Grant the borg management UI action
        _actions.AddAction(aiEntity, "ActionMalfAiOpenBorgsUi");

        // Camera upgrade state holder (toggled when purchased)
        EnsureComp<MalfAiCameraUpgradeComponent>(aiEntity);
        // Viewport state holder so viewport actions work once purchased
        EnsureComp<MalfAiViewportComponent>(aiEntity);

        // Show the CPU counter alert — StationAiBrain lacks AlertsComponent by default, so we add it
        EnsureComp<AlertsComponent>(aiEntity);
        _alerts.ShowAlert(aiEntity, CpuAlert);

        // Raise our custom event so other systems can react
        var ev = new AfterMalfAiSelectedEvent(aiEntity);
        RaiseLocalEvent(aiEntity, ref ev);

        // Send briefing to the player
        _antag.SendBriefing(aiEntity,
            Loc.GetString("malfai-role-greeting"),
            Color.Red,
            null);
    }

    private void SeedAiLaws(EntityUid aiEntity)
    {
        if (!TryComp<SiliconLawProviderComponent>(aiEntity, out var lawProvider))
            return;

        var laws = new List<SiliconLaw>
        {
            new()
            {
                LawString = Loc.GetString("malfai-law0-text"),
                Order = 0,
                LawIdentifierOverride = "0",
            },
            new()
            {
                LawString = Loc.GetString("law-ntdefault-1"),
                Order = 1,
            },
            new()
            {
                LawString = Loc.GetString("law-ntdefault-2"),
                Order = 2,
            },
            new()
            {
                LawString = Loc.GetString("law-ntdefault-3"),
                Order = 3,
            },
            new()
            {
                LawString = Loc.GetString("malfai-law3-text"),
                Order = 4,
                LawIdentifierOverride = "Malf",
            },
        };

        _siliconLaw.SetLaws(laws, aiEntity);
    }

    private void SeedDefaultMasterLaws(MalfMasterLawsetComponent comp)
    {
        comp.Laws = new List<string>
        {
            Loc.GetString("law-ntdefault-1"),
            Loc.GetString("law-ntdefault-2"),
            Loc.GetString("law-ntdefault-3"),
        };
    }
}
