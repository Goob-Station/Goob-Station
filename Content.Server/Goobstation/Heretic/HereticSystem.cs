using Content.Server.Actions;
using Content.Server.Goobstation.Objectives.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Store.Components;

namespace Content.Server.Heretic;

public sealed partial class HereticSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticComponent, ComponentInit>(OnCompInit);

        SubscribeAbilities();
    }

    public void UpdateKnowledge(EntityUid uid, HereticComponent comp, float amount)
    {
        if (TryComp<StoreComponent>(uid, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "KnowledgePoint", amount } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
                objective.Researched += amount;
    }

    public void OnCompInit(Entity<HereticComponent> ent, ref ComponentInit args)
    {
        foreach (var ability in ent.Comp.BaseActions)
            _action.AddAction(ent, ability);
    }
}
