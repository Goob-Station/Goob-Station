using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Power.Components;
using Content.Server.Store.Systems;
using Content.Shared.Mind;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfStationAIComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MalfStationAIComponent, MalfAIOpenShopAction>(OnToggleShop);
        SubscribeLocalEvent<ApcComponent, OnHackedEvent>(OnAPCHacked);

        InitializeActions();
        InitializeObjectives();
    }

    private void OnStartup(Entity<MalfStationAIComponent> entity, ref ComponentStartup args)
    {
        _actions.AddAction(entity, entity.Comp.MalfAIToggleShopAction);

        // Add the starting amount of processing power to the store balance.
        AddProcessingPower(entity, entity.Comp.StartingProcessingPower);
    }

    private void OnToggleShop(Entity<MalfStationAIComponent> entity, ref MalfAIOpenShopAction args)
    {
        if (!TryComp<StoreComponent>(entity, out var store))
            return;
        _store.ToggleUi(entity, entity, store);
    }

    public void AddProcessingPower(Entity<MalfStationAIComponent> entity, FixedPoint2 amount)
    {
        if (!TryComp<StoreComponent>(entity, out var store))
            return;

        if (!_store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { entity.Comp.ProcessingPowerPrototype, amount } }, entity))
            return;

        _store.UpdateUserInterface(entity, entity, store);
    }

    public bool TryRemoveProcessingPower(Entity<MalfStationAIComponent> entity, FixedPoint2 amount)
    {
        // There is no method in the store class for
        // removing currency that I know of and I don't
        // want to touch store code so here this goes.

        if (!TryComp<StoreComponent>(entity, out var store))
            return false;

        if (!store.Balance.ContainsKey(entity.Comp.ProcessingPowerPrototype))
            return false;

        if (store.Balance[entity.Comp.ProcessingPowerPrototype] < amount)
            return false;

        store.Balance[entity.Comp.ProcessingPowerPrototype] -= amount;

        _store.UpdateUserInterface(entity, entity, store);

        return true;
    }

    private void OnAPCHacked(Entity<ApcComponent> ent, ref OnHackedEvent args)
    {
        var malfComp = args.HackerEntity.Comp;

        // This is so that the APC updates its screen to be emagged looking or whatever I dunno man I don't even work here why are you asking me? I mean I dont even know why the APC component is serverside in the first place.
        ent.Comp.NeedStateUpdate = true;

        AddProcessingPower(args.HackerEntity, malfComp.HackAPCReward);
    }

    public bool IsAIAliveAndOnStation(EntityUid entity, EntityUid targetStation)
    {
        // If the core can't be found then the AI is probably ic "dead"
        return _stationAi.TryGetCore(entity, out var core)
            && _station.GetOwningStation(core) is { } station
            && station == targetStation;
    }
}