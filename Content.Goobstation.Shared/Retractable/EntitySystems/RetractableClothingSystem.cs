using Content.Goobstation.Shared.Retractable.Components;
using Content.Goobstation.Shared.Retractable.Events;
using Content.Shared.Actions;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.RetractableItemAction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Retractable.EntitySystems;

public sealed class RetractableClothingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RetractableClothingActionComponent, MapInitEvent>(OnActionInit);
        SubscribeLocalEvent<RetractableClothingActionComponent, RetractableClothingActionEvent>(OnRetractableClothingAction);
    }

    private void OnActionInit(Entity<RetractableClothingActionComponent> entity, ref MapInitEvent args)
    {
        _containerSystem.EnsureContainer<Container>(entity.Owner, entity.Comp.ContainerId);

        PopulateClothing(entity.Owner);
    }

    private void OnRetractableClothingAction(Entity<RetractableClothingActionComponent> entity, ref RetractableClothingActionEvent args)
    {
        var user = args.Performer;

        if (_actionsSystem.GetAction(entity.Owner) is not { } action)
            return;

        if (action.Comp.AttachedEntity == null || entity.Comp.ActionClothingUid.Count == 0)
            return;

        var requiresRetract = false;

        // Checks for every clothing happens here.
        foreach (var clothing in entity.Comp.ActionClothingUid)
        {
            // Check if user have slots for every retractable clothing and tell him if not!
            if (!_inventorySystem.HasSlot(user, clothing.Key))
            {
                _popupSystem.PopupClient(Loc.GetString("retractable-clothing-fail-no-slot", ("slot", clothing.Key)), user, user);
                return;
            }

            // Check if we can remove current clothing and current clothing is not our retractableClothing
            if (_inventorySystem.TryGetSlotEntity(args.Performer, clothing.Key, out var inSlotClothing) &&
                inSlotClothing != clothing.Value &&
                !_inventorySystem.CanUnequip(args.Performer, clothing.Key, out var reason))
            {
                _popupSystem.PopupClient(Loc.GetString("retractable-clothing-fail-unequip-current", ("slot", clothing.Key)), args.Performer, args.Performer);
                return;
            }

            // Check if we need retract or summon clothing. If any clothing is summoned right now (for some reason) - system will retract all of them first.
            if (_inventorySystem.TryGetSlotEntity(args.Performer, clothing.Key, out inSlotClothing) && inSlotClothing == clothing.Value)
                requiresRetract = true;
        }

        if (requiresRetract)
        {
            RetractClothings(args.Performer, entity.Owner);
        }
        else
        {
            SummonClothings(args.Performer, entity.Owner);
        }

        args.Handled = true;
    }

    public void PopulateClothing(Entity<RetractableClothingActionComponent?> entity)
    {
        if (!Resolve(entity.Owner, ref entity.Comp, false) || TerminatingOrDeleted(entity))
            return;

        var summonedList = new Dictionary<string, EntityUid>();

        // Try to spawn every clothing and insert them in container first
        foreach (var entry in entity.Comp.SpawnedPrototypes)
        {
            // Check if we already have clothing that does not requires spawning
            if (entity.Comp.ActionClothingUid.ContainsKey(entry.Key))
                continue;

            if (!PredictedTrySpawnInContainer(entry.Value, entity.Owner, entity.Comp.ContainerId, out var summoned))
            {
                // Remove all entities if we failed to spawn at least one to show that there is a problem
                foreach (var ent in summonedList)
                    PredictedQueueDel(ent.Value);

                return;
            }

            summonedList.Add(entry.Key, summoned.Value);
        }

        foreach(var summoned in summonedList)
        {
            entity.Comp.ActionClothingUid.Add(summoned.Key, summoned.Value);

            var summonedComp = EnsureComp<RetractableClothingComponent>(summoned.Value);
            summonedComp.SummoningAction = entity.Owner;

            Dirty(summoned.Value, summonedComp);
        }

        summonedList.Clear();

        Dirty(entity);
    }

    public void RetractClothings(EntityUid user, Entity<RetractableClothingActionComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        var container = _containerSystem.GetContainer(action, action.Comp.ContainerId);

        foreach (var clothing in action.Comp.ActionClothingUid.Values)
        {
            RemComp<UnremoveableComponent>(clothing);
            _containerSystem.Insert(clothing, container);
        }

        _audioSystem.PlayPredicted(action.Comp.RetractSounds, user, user);
    }

    public void SummonClothings(EntityUid user, Entity<RetractableClothingActionComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        var failedToEquip = false;
        foreach (var clothing in action.Comp.ActionClothingUid)
        {
            _inventorySystem.TryUnequip(user, clothing.Key, force: true, predicted: true);

            if (!_inventorySystem.TryEquip(user, clothing.Value, clothing.Key, force: true, predicted: true))
            {
                failedToEquip = true;

                // Continue to prevent ensuring unremoveable component on clothing that wasn't summoned
                continue;
            }

            EnsureComp<UnremoveableComponent>(clothing.Value);
        }

        if (!failedToEquip)
            _audioSystem.PlayPredicted(action.Comp.SummonSounds, user, user);
    }
}
