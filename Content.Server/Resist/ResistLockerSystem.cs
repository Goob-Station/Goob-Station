// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Lock;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Resist;
using Content.Shared.Storage.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.ActionBlocker;
using Robust.Server.Containers;

namespace Content.Server.Resist;

public sealed class ResistLockerSystem : EntitySystem
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly LockSystem _lockSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly WeldableSystem _weldable = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ContainerSystem _container = default!; // good edit

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ResistLockerComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<ResistLockerComponent, ResistLockerDoAfterEvent>(OnDoAfter);
    }

    // goob edit - made it support more than just entity storage
    private void OnRelayMovement(EntityUid uid, ResistLockerComponent component, ref ContainerRelayMovementEntityEvent args)
    {
        if (component.IsResisting // already resisting
        || !_actionBlocker.CanMove(args.Entity) // can move
        || TryComp<LockComponent>(uid, out var @lock) && !@lock.Locked // has a lock and is unlocked
        || !_weldable.IsWelded(uid)) // not welded
            return;

        AttemptResist(args.Entity, uid, component);
    }

    private void AttemptResist(EntityUid user, EntityUid target, ResistLockerComponent? resistLockerComponent = null)
    {
        if (!Resolve(target, ref resistLockerComponent))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, user, resistLockerComponent.ResistTime, new ResistLockerDoAfterEvent(), target, target: target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false, //No hands 'cause we be kickin'
        };

        // Make sure the do after is able to start
        if (!_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
            return;

        resistLockerComponent.IsResisting = true;
        _popupSystem.PopupEntity(Loc.GetString("resist-locker-component-start-resisting"), user, user, PopupType.Large);
    }

    private void OnDoAfter(EntityUid uid, ResistLockerComponent component, DoAfterEvent args)
    {
        if (args.Cancelled)
        {
            component.IsResisting = false;
            _popupSystem.PopupEntity(Loc.GetString("resist-locker-component-resist-interrupted"), args.Args.User, args.Args.User, PopupType.Medium);
            return;
        }

        if (args.Handled || args.Args.Target == null)
            return;

        component.IsResisting = false;

        if (HasComp<EntityStorageComponent>(uid))
        {
            WeldableComponent? weldable = null;
            if (_weldable.IsWelded(uid, weldable))
                _weldable.SetWeldedState(uid, false, weldable);

            if (TryComp<LockComponent>(args.Args.Target.Value, out var lockComponent))
                _lockSystem.Unlock(uid, args.Args.User, lockComponent);

            _entityStorage.TryOpenStorage(args.Args.User, uid);

            args.Handled = true; // goob edit
            return;
        }

        _container.TryRemoveFromContainer(args.Args.User, true); // goob edit
        args.Handled = true;
    }
}
