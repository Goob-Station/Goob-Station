using Content.Server.Actions;
using Content.Shared.Popups;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Handles summoning a meat spike at the slasher's position.
/// </summary>
public sealed class SlasherSummonMeatSpikeSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, SlasherSummonMeatSpikeEvent>(OnSummon);
    }

    private void OnMapInit(Entity<SlasherSummonMeatSpikeComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<SlasherSummonMeatSpikeComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnSummon(Entity<SlasherSummonMeatSpikeComponent> ent, ref SlasherSummonMeatSpikeEvent args)
    {
        Spawn(ent.Comp.MeatSpikePrototype, _xform.GetMoverCoordinates(ent.Owner));
        _popup.PopupEntity(Loc.GetString("slasher-summon-meatspike-popup"), ent.Owner, ent.Owner, PopupType.MediumCaution);
        args.Handled = true;
    }
}
