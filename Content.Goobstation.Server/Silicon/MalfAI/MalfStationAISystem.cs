using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Server.Actions;
using Content.Server.Power.Components;
using Content.Server.Store.Systems;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfStationAIComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MalfStationAIComponent, MalfAIOpenShopAction>(OnToggleShop);
        SubscribeLocalEvent<ApcComponent, OnHackedEvent>(OnAPCHacked);
    }

    private void OnStartup(Entity<MalfStationAIComponent> ent, ref ComponentStartup args)
    {
        _actions.AddAction(ent, ent.Comp.MalfAIToggleShopAction);
    }

    private void OnToggleShop(Entity<MalfStationAIComponent> ent, ref MalfAIOpenShopAction args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;
        _store.ToggleUi(ent, ent, store);
    }

    public void AddProcessingPower(Entity<MalfStationAIComponent> entity, FixedPoint2 amount)
    {
        if (!TryComp<StoreComponent>(entity, out var store))
            return;

        if (!_store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { entity.Comp.ProcessingPowerPrototype, amount } }, entity))
            return;

        _store.UpdateUserInterface(entity, entity, store);
    }

    private void OnAPCHacked(Entity<ApcComponent> ent, ref OnHackedEvent args)
    {
        var malfComp = args.HackerEntity.Comp;

        // This is so that the APC updates its screen to be emagged looking or whatever I dunno man I don't even work here why are you asking me? I mean I dont even know why the APC component is serverside in the first place.
        ent.Comp.NeedStateUpdate = true;

        AddProcessingPower(args.HackerEntity, malfComp.HackAPCReward);
    }
}