using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._White.RadialSelector;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class SummonVoidCreatureSystem : EntitySystem
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
        SubscribeLocalEvent<SummonVoidCreatureComponent, RadialSelectorSelectedMessage>(OnSummonVoidCreatureSelected);
    }

    private void OnMapInit(Entity<SummonVoidCreatureComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<SummonVoidCreatureComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnSummonVoidCreature(Entity<SummonVoidCreatureComponent> ent, ref SummonVoidCreatureEvent args)
    {
        if (args.Handled)
            return;

        _ui.TryToggleUi(ent.Owner, RadialSelectorUiKey.Key, ent.Owner);
        _ui.SetUiState(ent.Owner, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(ent.Comp.AvailableSummons));

        args.Handled = true;
    }

    private void OnSummonVoidCreatureSelected(Entity<SummonVoidCreatureComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        if (args.SelectedItem is not { } proto || !_proto.TryIndex(proto, out _))
            return;

        var uid = ent.Owner;
        var coordinates = _transform.GetMoverCoordinates(uid);

        var summoned = Spawn(proto, coordinates);

        // Optionally transfer mind if this is a controllable summon
        if (_mind.TryGetMind(uid, out var mindUid, out var mind))
        {
            _mind.TransferTo(mindUid, summoned, mind);
            _mind.UnVisit(mindUid, mind);
        }

        // Copy components from summoner if desired (optional)
        EntityManager.CopyComponents(uid, summoned);

        _ui.CloseUi(ent.Owner, RadialSelectorUiKey.Key, args.Actor);
    }
}
