using Content.Shared.Clothing;
using Content.Shared.Actions;
using Content.Shared.Light.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Clothing.Systems
{

    public sealed class HandheldLightItemSlotSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            // Listen for when a player uses an item on the helmet
            SubscribeLocalEvent<HandheldLightItemSlotComponent, GotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, GotUnequippedEvent>(OnGotUnequipped);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, EntRemovedFromContainerMessage>(OnLightRemoved);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, EntInsertedIntoContainerMessage>(OnLightInserted);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, ComponentGetState>(OnGetState);
        }

        private void OnGetState(EntityUid uid, HandheldLightItemSlotComponent component, ref ComponentGetState args)
        {
            if (component.AttachedLight == null)
            {
                args.State = new HandheldLightItemSlotComponentState(null);
                return;
            }
            if (!_appearance.TryGetData(uid, HandheldLightVisuals.Power, out HandheldLightPowerStates value)
            || !TryComp(uid, out HandheldLightComponent? lightComponent))
                return;

            args.State = new HandheldLightItemSlotComponentState(new LightState
            {
                Activated = lightComponent.Activated,
                PowerState = value
            });
        }

        private void OnLightRemoved(EntityUid uid, HandheldLightItemSlotComponent component, EntRemovedFromContainerMessage args)
        {
            if (args.Container.ID != component.SlotName || !TryComp(args.Entity, out HandheldLightComponent? lightComponent))
                return;

            // If worn on head by the action taker then remove the securityhelmet.SecliteEntity action via RemoveAction.
            if (component.Wearer != null && component.EntityActionReference != null)
                _actionsSystem.RemoveAction(component.Wearer.Value, component.EntityActionReference);

            component.AttachedLight = null;
        }
        private void OnLightInserted(EntityUid uid, HandheldLightItemSlotComponent component, EntInsertedIntoContainerMessage args)
        {
            if (args.Container.ID != component.SlotName || !TryComp(args.Entity, out HandheldLightComponent? lightComponent))
                return;

            // If worn on head by the action taker add the lightcomponent.toggleaction via addAction.
            if (component.Wearer != null)
                _actionsSystem.AddAction(component.Wearer.Value, ref component.EntityActionReference, lightComponent.ToggleAction, args.Entity);

            component.AttachedLight = args.Entity;
        }
        private void OnGotEquipped(EntityUid uid, HandheldLightItemSlotComponent component, GotEquippedEvent args)
        {
            var lightOption = _itemSlotsSystem.GetItemOrNull(uid, component.SlotName);
            component.Wearer = args.Equipee;

            if (lightOption != null && TryComp(lightOption, out HandheldLightComponent? lightComponent))
                _actionsSystem.AddAction(args.Equipee, ref component.EntityActionReference, lightComponent.ToggleAction, lightOption.Value);
        }
        private void OnGotUnequipped(EntityUid uid, HandheldLightItemSlotComponent component, GotUnequippedEvent args)
        {
            var lightOption = _itemSlotsSystem.GetItemOrNull(uid, component.SlotName);
            component.Wearer = null;

            if (lightOption == null || component.EntityActionReference == null)
                return;

            _actionsSystem.RemoveAction(args.Equipee, component.EntityActionReference);
        }
    }
}
