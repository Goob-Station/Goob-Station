using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed partial class ChangelingEquipmentSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingEquipmentComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, DroppedEvent>(OnDropped);

        SubscribeLocalEvent<ChangelingEquipmentComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);

        SubscribeLocalEvent<ChangelingEquipmentComponent, InventoryRelayedEvent<ChangelingChemicalRegenEvent>>(OnChangelingChemicalRegenEvent);
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

    private void OnChangelingChemicalRegenEvent(Entity<ChangelingEquipmentComponent> ent, ref InventoryRelayedEvent<ChangelingChemicalRegenEvent> args)
    {
        if (ent.Comp.User != null)
            args.Args.Modifier -= ent.Comp.ChemModifier;
    }
}
