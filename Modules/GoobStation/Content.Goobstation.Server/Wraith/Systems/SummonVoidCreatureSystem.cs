using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._White.RadialSelector;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class SummonVoidCreatureSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonVoidCreatureComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SummonVoidCreatureComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SummonVoidCreatureComponent, SummonVoidCreatureEvent>(OnSummonVoidCreature);

        SubscribeLocalEvent<ChooseVoidCreatureComponent, ChooseVoidCreatureEvent>(OnChooseVoidCreature);
        SubscribeLocalEvent<ChooseVoidCreatureComponent, RadialSelectorSelectedMessage>(OnSummonVoidCreatureSelected);
    }

    private void OnMapInit(Entity<SummonVoidCreatureComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<SummonVoidCreatureComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnSummonVoidCreature(Entity<SummonVoidCreatureComponent> ent, ref SummonVoidCreatureEvent args)
    {
        SpawnAtPosition(ent.Comp.SummonId, Transform(ent.Owner).Coordinates);

        args.Handled = true;
    }

    private void OnChooseVoidCreature(Entity<ChooseVoidCreatureComponent> ent, ref ChooseVoidCreatureEvent args)
    {
        _ui.TryToggleUi(ent.Owner, RadialSelectorUiKey.Key, ent.Owner);
        _ui.SetUiState(ent.Owner, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(ent.Comp.AvailableSummons));
    }

    private void OnSummonVoidCreatureSelected(Entity<ChooseVoidCreatureComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        if (args.SelectedItem is not { } proto || !_proto.TryIndex(proto, out _)
            || !_mind.TryGetMind(ent.Owner, out var mindUid, out var mind))
            return;

        var coordinates = _transform.GetMoverCoordinates(ent.Owner);
        var newForm = Spawn(proto, coordinates);

        _mind.TransferTo(mindUid, newForm, mind: mind);
        _mind.UnVisit(mindUid, mind);

        EntityManager.CopyComponents(ent.Owner, newForm);
        RemComp<ChooseVoidCreatureComponent>(newForm);

        _ui.CloseUi(ent.Owner, RadialSelectorUiKey.Key, args.Actor);
        Del(ent.Owner);
    }
}
