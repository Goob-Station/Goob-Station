using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Server.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherSummonMacheteSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSummonMacheteComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSummonMacheteComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSummonMacheteComponent, SlasherSummonMacheteEvent>(OnSummon);
    }

    private void OnMapInit(Entity<SlasherSummonMacheteComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherSummonMacheteComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    /// <summary>
    /// Slasher - Handles summoning the Machete
    /// </summary>
    private void OnSummon(Entity<SlasherSummonMacheteComponent> ent, ref SlasherSummonMacheteEvent args)
    {
        // Fail if the user has no hands.
        if (!TryComp<HandsComponent>(ent.Owner, out var hands) || hands.Hands.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("wieldable-component-no-hands"), ent.Owner, ent.Owner);
            args.Handled = true;
            return;
        }

        // Ensure we have or create the machete
        var machete = ent.Comp.MacheteUid;

        if (machete == null || Deleted(machete))
        {
            if (!_protos.TryIndex(ent.Comp.MachetePrototype, out EntityPrototype? _))
                return;

            machete = Spawn(ent.Comp.MachetePrototype, _xform.GetMoverCoordinates(ent.Owner));
            ent.Comp.MacheteUid = machete;
            Dirty(ent);
        }

        _hands.TryPickupAnyHand(ent.Owner, machete.Value);

        args.Handled = true;
    }
}
