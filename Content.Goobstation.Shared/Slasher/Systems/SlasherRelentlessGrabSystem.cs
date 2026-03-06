using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Common.Weapons;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the Slasher Relentless Grab action.
/// When activated, the slasher's next melee hit will grab the target.
/// </summary>
public sealed class SlasherRelentlessGrabSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRelentlessGrabComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherRelentlessGrabComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherRelentlessGrabComponent, SlasherRelentlessGrabEvent>(OnActivate);
        SubscribeLocalEvent<SlasherRelentlessGrabComponent, LightAttackSpecialInteractionEvent>(OnLightAttackSpecial);
    }

    private void OnMapInit(Entity<SlasherRelentlessGrabComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherRelentlessGrabComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    private void OnActivate(Entity<SlasherRelentlessGrabComponent> ent, ref SlasherRelentlessGrabEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Ready = true;
        Dirty(ent);

        _popup.PopupPredicted(Loc.GetString("slasher-relentless-grab-activate"), ent.Owner, ent.Owner);

        args.Handled = true;
    }

    private void OnLightAttackSpecial(Entity<SlasherRelentlessGrabComponent> ent, ref LightAttackSpecialInteractionEvent args)
    {
        if (!ent.Comp.Ready || args.Target == null)
            return;

        if (!_pulling.CanGrab(args.User, args.Target.Value))
            return;

        if (!_pulling.TryStartPull(args.User, args.Target.Value, grabStageOverride: GrabStage.Hard, force: true))
            return;

        ent.Comp.Ready = false;
        Dirty(ent);
    }
}
