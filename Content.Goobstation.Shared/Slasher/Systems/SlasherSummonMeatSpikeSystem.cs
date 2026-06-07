using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles summoning a meat spike at the slasher's position.
/// </summary>
public sealed class SlasherSummonMeatSpikeSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSummonMeatSpikeComponent, SlasherSummonMeatSpikeEvent>(OnSummon);
    }

    private void OnMapInit(Entity<SlasherSummonMeatSpikeComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherSummonMeatSpikeComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    private void OnSummon(Entity<SlasherSummonMeatSpikeComponent> ent, ref SlasherSummonMeatSpikeEvent args)
    {
        _audio.PlayPredicted(ent.Comp.SummonSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(Loc.GetString("slasher-summon-meatspike-popup"), ent.Owner, ent.Owner, PopupType.MediumCaution);
        PredictedSpawnAtPosition(ent.Comp.MeatSpikePrototype, _xform.GetMoverCoordinates(ent.Owner));
        args.Handled = true;
    }
}
