using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._White.RadialSelector;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wraith;

/// <summary>
/// This handles evolving into a higher form with Wraith.
/// </summary>
public sealed class WraithEvolveSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EvolveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EvolveComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<EvolveComponent, WraithEvolveEvent>(OnWraithEvolve);
        SubscribeLocalEvent<EvolveComponent, RadialSelectorSelectedMessage>(OnWraithEvolveRecieved);
    }

    private void OnMapInit(Entity<EvolveComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<EvolveComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnWraithEvolve(Entity<EvolveComponent> ent, ref WraithEvolveEvent args)
    {
        // UNCOMMENT THIS ONCE DONE WITH PLAYTESTING!!!!!!!!!!!!
        /*if (!TryComp<AbsorbCorpseComponent>(ent.Owner, out var corpse)
            || corpse.CorpsesAbsorbed < ent.Comp.CorpsesRequired)
            return;*/

        _ui.TryToggleUi(ent.Owner, RadialSelectorUiKey.Key, ent.Owner);
        _ui.SetUiState(ent.Owner, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(ent.Comp.AvailableEvolutions));

        args.Handled = true;
    }

    private void OnWraithEvolveRecieved(Entity<EvolveComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        Evolve(ent, args.SelectedItem);

        _ui.CloseUi(ent.Owner, RadialSelectorUiKey.Key, args.Actor);
    }

    private void Evolve(Entity<EvolveComponent> ent, string? evolve)
    {
        var uid = ent.Owner;
        if (evolve == null
            || !_proto.TryIndex(evolve, out _)
            || !_mind.TryGetMind(uid, out var mindUid, out var mind))
            return;

        var coordinates = _transformSystem.GetMoverCoordinates(uid);
        var newForm = Spawn(evolve, coordinates);

        _mind.TransferTo(mindUid, newForm, mind: mind);
        _mind.UnVisit(mindUid, mind);

        EntityManager.CopyComponents(uid, newForm);

        RemComp<EvolveComponent>(newForm);
        Del(uid);
    }
}
