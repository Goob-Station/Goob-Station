using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Shared._White.RadialSelector;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
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
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EvolveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EvolveComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<EvolveComponent, WraithEvolveEvent>(OnWraithEvolve);
        SubscribeLocalEvent<EvolveComponent, RadialSelectorSelectedMessage>(OnWraithEvolveRecieved);

        SubscribeLocalEvent<AbsorbCorpseComponent, WraithEvolveAttemptEvent>(OnWraithEvolveAttempt);
    }

    private void OnMapInit(Entity<EvolveComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<EvolveComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnWraithEvolve(Entity<EvolveComponent> ent, ref WraithEvolveEvent args)
    {
        var ev = new WraithEvolveAttemptEvent(ent.Comp.CorpsesRequired);
        RaiseLocalEvent(ent, ref ev);

        if (ev.Cancelled)
            return;

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

        var meta = MetaData(uid);
        _meta.SetEntityName(newForm, meta.EntityName);

        _mind.TransferTo(mindUid, newForm, mind: mind);
        _mind.UnVisit(mindUid, mind);

        EntityManager.CopyComponents(uid, newForm);

        _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} evolved to {ToPrettyString(newForm)} as a Wraith");

        RemComp<EvolveComponent>(newForm);
        Del(uid);
    }

    private void OnWraithEvolveAttempt(Entity<AbsorbCorpseComponent> ent, ref WraithEvolveAttemptEvent args)
    {
        if (ent.Comp.CorpsesAbsorbed < args.CorpsesRequired)
        {
            _popups.PopupEntity(Loc.GetString("wraith-evolve-not-enough", ("corpseCount", args.CorpsesRequired)), ent.Owner, ent.Owner);
            args.Cancelled = true;
        }
    }
}
