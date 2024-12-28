using Content.Server.Storage.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared._Goobstation.ChronoLegionnaire;
using Content.Shared._Goobstation.ChronoLegionnaire.Components;
using Content.Shared.Interaction;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.ChronoLegionnaire;

public sealed class TimelineDisruptorSystem : SharedTimelineDisruptorSystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimelineDisruptorComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<TimelineDisruptorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TimelineDisruptorComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<TimelineDisruptorComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
    }

    /// <summary>
    /// Making verb to start disrupting procedure
    /// </summary>
    private void OnActivate(Entity<TimelineDisruptorComponent> ent, ref ActivateInWorldEvent args)
    {
        var comp = ent.Comp;

        if (args.Handled || !args.Complex)
            return;

        if (!_itemSlotsSystem.TryGetSlot(ent, comp.DisruptionSlot, out var disruptionSlot))
            return;

        if (disruptionSlot.ContainerSlot == null || disruptionSlot.ContainerSlot.ContainedEntities.Count == 0)
            return;

        StartDisrupting(ent);

        args.Handled = true;
    }

    private bool TryCheckContainer(Entity<TimelineDisruptorComponent> ent)
    {
        if (!_containerSystem.TryGetContainer(ent, ent.Comp.DisruptionSlot, out var container))
            return false;

        if (container.ContainedEntities.Count == 0)
            return false;

        return true;
    }

    private void OnMapInit(Entity<TimelineDisruptorComponent> ent, ref MapInitEvent args)
    {
        bool isContain = TryCheckContainer(ent);

        UpdateContainerAppearance(ent, isContain);
    }

    private void UpdateContainerAppearance(Entity<TimelineDisruptorComponent> ent, bool isContain, AppearanceComponent? appearance = null)
    {
        if (!Resolve(ent, ref appearance, false))
            return;

        _appearanceSystem.SetData(ent, TimelineDisruptiorVisuals.ContainerInserted, isContain, appearance);
    }

    private void OnContainerChanged(EntityUid uid, TimelineDisruptorComponent component, ContainerModifiedMessage args)
    {
        bool isContain = TryCheckContainer((uid, component));

        UpdateContainerAppearance((uid, component), isContain);
    }

    private void StartDisrupting(Entity<TimelineDisruptorComponent> ent)
    {
        var (uid, disruptor) = ent;

        if (disruptor.Disruption)
            return;

        disruptor.Disruption = true;
        disruptor.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);
        disruptor.DisruptionEndTime = _timing.CurTime + disruptor.DisruptionDuration;
        disruptor.DisruptionSoundStream = _audioSystem.PlayPredicted(disruptor.DusruptionSound, ent, null)?.Entity;

        _appearanceSystem.SetData(ent, TimelineDisruptiorVisuals.Disrupting, true);
        Dirty(uid, disruptor);
    }

    protected void StopDisrupting(Entity<TimelineDisruptorComponent> ent)
    {
        var (_, disruptor) = ent;

        if (!disruptor.Disruption)
            return;

        disruptor.Disruption = false;
        _appearanceSystem.SetData(ent, TimelineDisruptiorVisuals.Disrupting, false);

        Dirty(ent, ent.Comp);
    }
    private void FinishDisrupting(Entity<TimelineDisruptorComponent> ent)
    {
        var (_, disruptor) = ent;
        StopDisrupting(ent);

        Dirty(ent, disruptor);

        if (!_itemSlotsSystem.TryGetSlot(ent, disruptor.DisruptionSlot, out var disruptionSlot))
            return;

        EntityUid? cage = disruptionSlot.ContainerSlot!.ContainedEntity;

        if (cage == null)
            return;

        // Checking the storage of stasis container for any items in it
        if (!TryComp<EntityStorageComponent>(cage, out var entityStorage) || entityStorage.Contents.ContainedEntities.Count == 0)
            return;

        var contents = new List<EntityUid>(entityStorage.Contents.ContainedEntities);
        foreach (var contained in contents)
        {
            // Removing entity from container to delete it without ghost breaking
            _containerSystem.RemoveEntity(cage.Value, contained);
            QueueDel(contained);
        }

        disruptor.DisruptionSoundStream = _audioSystem.Stop(disruptor.DisruptionSoundStream);
        _audioSystem.PlayPredicted(disruptor.DisruptionCompleteSound, ent, null);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TimelineDisruptorComponent>();
        while (query.MoveNext(out var uid, out var disruptor))
        {
            if (!disruptor.Disruption)
                continue;

            if (!_itemSlotsSystem.TryGetSlot(uid, disruptor.DisruptionSlot, out var disruptionSlot))
                continue;

            // Check if we removed stasis container from disruptor
            if ((disruptionSlot.ContainerSlot == null || disruptionSlot.ContainerSlot.ContainedEntity == null) && disruptor.Disruption)
            {
                StopDisrupting((uid, disruptor));
                disruptor.DisruptionSoundStream = _audioSystem.Stop(disruptor.DisruptionSoundStream);
                continue;
            }

            if (disruptor.NextSecond < _timing.CurTime)
            {
                disruptor.NextSecond += TimeSpan.FromSeconds(1);
                Dirty(uid, disruptor);
            }

            if (disruptor.DisruptionEndTime < _timing.CurTime)
                FinishDisrupting((uid, disruptor));
        }
    }
}
