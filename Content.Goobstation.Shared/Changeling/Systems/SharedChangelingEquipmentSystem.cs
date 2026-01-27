using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed class ChangelingEquipmentSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingEquipmentComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, DroppedEvent>(OnDropped);

        SubscribeLocalEvent<ChangelingEquipmentComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);

        SubscribeLocalEvent<ChangelingEquipmentComponent, InventoryRelayedEvent<InternalResourcesRegenModifierEvent>>(OnChangelingChemicalRegenEvent);
    }

    private void OnEquipped(Entity<ChangelingEquipmentComponent> ent, ref GotEquippedEvent args)
    {
        if (ent.Comp.RequiredSlot != null
            && !args.SlotFlags.HasFlag(ent.Comp.RequiredSlot))
            return;

        ent.Comp.User = args.Equipee;

        Dirty(ent);
    }

    private void OnUnequipped(Entity<ChangelingEquipmentComponent> ent, ref GotUnequippedEvent args)
    {
        PredictedQueueDel(ent.Owner);
    }

    private void OnDropped(Entity<ChangelingEquipmentComponent> ent, ref DroppedEvent args)
    {
        PredictedQueueDel(ent.Owner);
    }

    private void OnRemoveAttempt(Entity<ChangelingEquipmentComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        if (!_gameTiming.ApplyingState)
            args.Cancel();
    }

    private void OnChangelingChemicalRegenEvent(Entity<ChangelingEquipmentComponent> ent, ref InventoryRelayedEvent<InternalResourcesRegenModifierEvent> args)
    {
        if (args.Args.Data.InternalResourcesType != ent.Comp.ResourceType
            || ent.Comp.User == null)
            return;

        args.Args.Modifier -= ent.Comp.ChemModifier;
    }
}
