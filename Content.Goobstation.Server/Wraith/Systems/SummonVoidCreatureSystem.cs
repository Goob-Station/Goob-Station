using Content.Goobstation.Shared.Wraith.Components;
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

        //  remove Evolve component if it exists, since it breaks Summon void creature. The wraith should not have the Evolve component anyway. This is just to prevent any potential edge cass issues.
        if (HasComp<EvolveComponent>(ent.Owner))
        {
            RemComp<EvolveComponent>(ent.Owner);
            Logger.Error($"[SummonVoidCreatureSystem] EvolveComponent removed from entity {ent.Owner}. Wraith should not have this component at this stage.");
        }

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

        _ui.CloseUi(ent.Owner, RadialSelectorUiKey.Key, args.Actor);
    }
}
