using System.Linq;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.Actions;
using Content.Shared.Localizations;
using Content.Shared.Lock;
using Content.Shared.Mech.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechLockSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AccessReaderSystem _accessReader = default!;
    [Dependency] private LockSystem _lock = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechLockComponent, LockToggleAttemptEvent>(OnLockToggleAttempt);
        SubscribeLocalEvent<MechLockComponent, LockToggledEvent>(OnLockToggled);
        SubscribeLocalEvent<MechComponent, MechToggleLockActionEvent>(OnPilotToggleLock);
        SubscribeLocalEvent<MechLockComponent, EntInsertedIntoContainerMessage>(OnPilotInserted);
        SubscribeLocalEvent<MechLockComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnPilotInserted(Entity<MechLockComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        _actions.AddAction(args.Entity, ref ent.Comp.ActionEntity, ent.Comp.ActionProto, ent);
        _actions.SetToggled(ent.Comp.ActionEntity, _lock.IsLocked(ent.Owner));
        Dirty(ent);
    }

    private void OnShutdown(Entity<MechLockComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
        PredictedQueueDel(ent.Comp.ActionEntity);
    }

    private static bool HasRequiredAccess(MechLockComponent mechLock, ICollection<ProtoId<AccessLevelPrototype>> tags) =>
        mechLock.RequiredAccess.Count == 0 || mechLock.RequiredAccess.Any(tags.Contains);

    private string NeedsAccessMessage(EntityUid mech, MechLockComponent mechLock)
    {
        var names = mechLock.RequiredAccess.Select(access => _proto.Index(access).GetAccessLevelName()).ToList();
        return Loc.GetString("mech-lock-needs-access", ("mech", mech), ("access", ContentLocalizationManager.FormatListToOr(names)));
    }

    private bool IsPilot(EntityUid mech, EntityUid user)
    {
        return TryComp<MechComponent>(mech, out var mechComp) && mechComp.PilotSlot.ContainedEntity == user;
    }

    private void OnLockToggleAttempt(Entity<MechLockComponent> ent, ref LockToggleAttemptEvent args)
    {
        if (args.Cancelled || !TryComp<LockComponent>(ent, out var lockComp))
            return;

        if (!lockComp.Locked && !IsPilot(ent, args.User))
        {
            args.Cancelled = true;
            return;
        }

        var tags = _accessReader.FindAccessTags(args.User);

        if (!lockComp.Locked && ent.Comp.RequireAccess)
        {
            if (!HasRequiredAccess(ent.Comp, tags))
            {
                if (!args.Silent)
                    _popup.PopupClient(NeedsAccessMessage(ent, ent.Comp), ent, args.User);
                args.Cancelled = true;
                return;
            }
        }
        else if (lockComp.Locked && ent.Comp.LockedAccess.Count > 0 && !IsPilot(ent, args.User) && !ent.Comp.LockedAccess.IsSubsetOf(tags))
        {
            if (!args.Silent)
                _popup.PopupClient(Loc.GetString("mech-lock-no-access", ("mech", ent.Owner)), ent, args.User);
            args.Cancelled = true;
            return;
        }

        ent.Comp.PendingUser = args.User;
    }

    private void OnLockToggled(Entity<MechLockComponent> ent, ref LockToggledEvent args)
    {
        ent.Comp.LockedAccess.Clear();

        if (args.Locked && ent.Comp.PendingUser is { } user && Exists(user))
            ent.Comp.LockedAccess.UnionWith(_accessReader.FindAccessTags(user));

        ent.Comp.PendingUser = null;
        Dirty(ent);
        _actions.SetToggled(ent.Comp.ActionEntity, args.Locked);
    }

    private void OnPilotToggleLock(Entity<MechComponent> ent, ref MechToggleLockActionEvent args)
    {
        if (args.Handled)
            return;

        var mech = ent.Owner;
        var pilot = args.Performer;
        if (!TryComp<MechLockComponent>(mech, out var mechLock)
            || !TryComp<LockComponent>(mech, out var lockComp))
            return;

        args.Handled = true;

        if (lockComp.Locked)
        {
            mechLock.PendingUser = pilot;
            _lock.Unlock(mech, pilot, lockComp);
            return;
        }

        if (mechLock.RequireAccess && !HasRequiredAccess(mechLock, _accessReader.FindAccessTags(pilot)))
        {
            _popup.PopupClient(NeedsAccessMessage(mech, mechLock), mech, pilot);
            return;
        }

        mechLock.PendingUser = pilot;
        _lock.Lock(mech, pilot, lockComp);
    }
}
