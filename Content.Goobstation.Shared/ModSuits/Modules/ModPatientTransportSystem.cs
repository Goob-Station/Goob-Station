using Content.Shared.Body.Events;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
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
    [Dependency] private SharedCuffableSystem _cuffable = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
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
        if (ent.Comp.RequireRestrained
            && !(TryComp<CuffableComponent>(target, out var cuffable) && _cuffable.IsCuffed((target, cuffable))))
        {
            _popup.PopupClient(Loc.GetString("modsuit-capture-not-restrained"), args.User, args.User);
            return;
        }

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

    [SubscribeLocalEvent]
    private void OnCaptureOpenAttempt(Entity<ModCaptureBagComponent> ent, ref StorageOpenAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnCaptureInserted(Entity<ModCaptureBagComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || args.Container.ID != SharedEntityStorageSystem.ContainerName || !HasComp<MobStateComponent>(args.Entity))
            return;

        var added = new List<string>();
        foreach (var (name, entry) in ent.Comp.CaptiveComponents)
        {
            if (EntityManager.HasComponent(args.Entity, entry.Component.GetType()))
                continue;

            added.Add(name);
        }

        EntityManager.AddComponents(args.Entity, ent.Comp.CaptiveComponents, false);
        ent.Comp.Added[args.Entity] = added;
    }

    [SubscribeLocalEvent]
    private void OnCaptureRemoved(Entity<ModCaptureBagComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_net.IsClient || args.Container.ID != SharedEntityStorageSystem.ContainerName)
            return;

        if (ent.Comp.Added.Remove(args.Entity, out var added) && !TerminatingOrDeleted(args.Entity))
        {
            foreach (var name in added)
            {
                if (ent.Comp.CaptiveComponents.TryGetValue(name, out var entry))
                    RemComp(args.Entity, entry.Component.GetType());
            }
        }

        if (HasComp<MobStateComponent>(args.Entity)
            && !TerminatingOrDeleted(args.Entity)
            && TryComp<StaminaComponent>(args.Entity, out var stamina))
            _stamina.TakeStaminaDamage(args.Entity, stamina.CritThreshold, stamina, ignoreResist: true);
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
