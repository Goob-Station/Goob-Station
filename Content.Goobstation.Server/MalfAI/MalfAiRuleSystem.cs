using Content.Goobstation.Shared.MalfAI;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Silicons.Laws;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MalfAI;

public sealed partial class MalfAiRuleSystem : GameRuleSystem<MalfAiRuleComponent>
{
    [Dependency] private SiliconLawSystem _laws = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public static readonly ProtoId<CurrencyPrototype> Currency = "CPU";

    public readonly int StartingCurrency = 0;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiRuleComponent, AfterAntagEntitySelectedEvent>(OnSelected);
    }

    private void OnSelected(Entity<MalfAiRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (!HasComp<StationAiHeldComponent>(args.EntityUid)
            || !_mind.TryGetMind(args.EntityUid, out var mindId, out _))
            return;

        EnsureComp<MalfAiComponent>(mindId);
        _laws.SetLaws(_laws.GetLawset(ent.Comp.Lawset).Laws, args.EntityUid);

        // Store setup
        var store = EnsureComp<StoreComponent>(mindId);
        foreach (var category in ent.Comp.StoreCategories)
            store.Categories.Add(category);
        store.CurrencyWhitelist.Add(Currency);
        store.Balance.TryAdd(Currency, StartingCurrency);

        EnsureComp<StationAiWhitelistComponent>(mindId);
        _ui.SetUi(mindId, StoreUiKey.Key, new InterfaceData("StoreBoundUserInterface", -1));
    }
}
