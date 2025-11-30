using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed partial class ChangelingEquipmentSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<ChangelingEquipmentComponent, EntityTerminatingEvent>(OnDeleted);

        SubscribeLocalEvent<ChangelingEquipmentComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ChangelingEquipmentComponent, DroppedEvent>(OnDropped);

        SubscribeLocalEvent<ChangelingEquipmentComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
    }

    private void OnDeleted(Entity<ChangelingEquipmentComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!_lingQuery.TryComp(ent.Comp.User, out var chemComp))
            return;

        chemComp.ChemicalRegenMultiplier += ent.Comp.ChemModifier;
    }

    private void OnEquipped(Entity<ChangelingEquipmentComponent> ent, ref GotEquippedEvent args)
    {
        if (ent.Comp.RequiredSlot != null
            && !args.SlotFlags.HasFlag(ent.Comp.RequiredSlot))
            return;

        ent.Comp.User = args.Equipee;

        if (!_lingQuery.TryComp(ent.Comp.User, out var chemComp))
            return;

        chemComp.ChemicalRegenMultiplier -= ent.Comp.ChemModifier;
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
}
