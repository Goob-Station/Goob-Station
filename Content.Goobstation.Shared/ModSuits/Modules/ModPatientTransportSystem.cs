using Content.Shared.Body.Events;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModPatientTransportSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private SharedMetabolizerSystem _metabolizer = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private readonly List<(EntityUid User, EntityUid Bag)> _pendingPulls = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingPulls.Count == 0)
            return;

        foreach (var (user, bag) in _pendingPulls)
        {
            if (TerminatingOrDeleted(bag))
                continue;

            if (TerminatingOrDeleted(user) || !_pulling.TryStartPull(user, bag))
                Dissolve(bag);
        }

        _pendingPulls.Clear();
    }

    [SubscribeLocalEvent]
    private void OnAfterInteract(Entity<ModPatientTransportComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (target == ent.Comp.LinkedBag)
        {
            args.Handled = true;
            Dissolve(target);
            return;
        }

        if (!HasComp<MobStateComponent>(target) || target == args.User)
            return;

        args.Handled = true;
        if (ent.Comp.LinkedBag != null)
        {
            _popup.PopupClient(Loc.GetString("modsuit-transport-already-linked"), args.User, args.User);
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.CaptureDelay, new ModPatientTransportDoAfterEvent(), ent, target, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        });
    }

    [SubscribeLocalEvent]
    private void OnDoAfter(Entity<ModPatientTransportComponent> ent, ref ModPatientTransportDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        args.Handled = true;
        if (_net.IsClient || ent.Comp.LinkedBag != null || TerminatingOrDeleted(target))
            return;

        var bag = Spawn(ent.Comp.BagPrototype, Transform(target).Coordinates);
        if (!_entityStorage.Insert(target, bag))
        {
            QueueDel(bag);
            return;
        }

        var bagComp = EnsureComp<ModHardlightBagComponent>(bag);
        bagComp.Projector = ent;
        Dirty(bag, bagComp);

        ent.Comp.LinkedBag = bag;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.Sound, bag);
        _pendingPulls.Add((args.User, bag));
    }

    [SubscribeLocalEvent]
    private void OnBagPullStopped(Entity<ModHardlightBagComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PulledUid == ent.Owner)
            Dissolve(ent);
    }

    [SubscribeLocalEvent]
    private void OnBagOpened(Entity<ModHardlightBagComponent> ent, ref StorageAfterOpenEvent args)
    {
        Dissolve(ent);
    }

    [SubscribeLocalEvent]
    private void OnBagShutdown(Entity<ModHardlightBagComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Projector is not { } projector
            || !TryComp<ModPatientTransportComponent>(projector, out var transport)
            || transport.LinkedBag != ent.Owner)
            return;

        transport.LinkedBag = null;
        Dirty(projector, transport);
    }

    [SubscribeLocalEvent]
    private void OnStasisInserted(Entity<ModStasisContainerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || args.Container.ID != SharedEntityStorageSystem.ContainerName || !HasComp<MobStateComponent>(args.Entity))
            return;

        var stasis = EnsureComp<ModInStasisComponent>(args.Entity);
        stasis.Multiplier = ent.Comp.Multiplier;
        Dirty(args.Entity, stasis);
        _metabolizer.UpdateMetabolicMultiplier(args.Entity);
    }

    [SubscribeLocalEvent]
    private void OnStasisRemoved(Entity<ModStasisContainerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_net.IsClient || args.Container.ID != SharedEntityStorageSystem.ContainerName || !RemComp<ModInStasisComponent>(args.Entity))
            return;

        _metabolizer.UpdateMetabolicMultiplier(args.Entity);
    }

    [SubscribeLocalEvent]
    private void OnGetMetabolicMultiplier(Entity<ModInStasisComponent> ent, ref GetMetabolicMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }

    private void Dissolve(EntityUid bag)
    {
        if (_net.IsClient || TerminatingOrDeleted(bag) || EntityManager.IsQueuedForDeletion(bag))
            return;

        if (TryComp<ModHardlightBagComponent>(bag, out var bagComp)
            && bagComp.Projector is { } projector
            && TryComp<ModPatientTransportComponent>(projector, out var transport))
        {
            transport.LinkedBag = null;
            Dirty(projector, transport);
            _audio.PlayPvs(transport.Sound, bag);
        }

        _entityStorage.EmptyContents(bag);
        QueueDel(bag);
    }
}

[Serializable, NetSerializable]
public sealed partial class ModPatientTransportDoAfterEvent : SimpleDoAfterEvent;
