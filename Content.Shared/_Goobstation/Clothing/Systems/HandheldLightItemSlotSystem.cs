using Content.Shared.Clothing;
using Content.Shared.Actions;
using Content.Shared.Light.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;

namespace Content.Server.GameObjects.Systems
{

    public sealed class HandheldLightItemSlotSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;


        public override void Initialize()
        {
            base.Initialize();

            // Listen for when a player uses an item on the helmet
            SubscribeLocalEvent<HandheldLightItemSlotComponent, GotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, GotUnequippedEvent>(OnGotUnequipped);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, EntRemovedFromContainerMessage>(OnLightRemoved);
            SubscribeLocalEvent<HandheldLightItemSlotComponent, EntInsertedIntoContainerMessage>(OnLightInserted);

        }
        private void OnLightRemoved(EntityUid uid, HandheldLightItemSlotComponent component, EntRemovedFromContainerMessage args)
        {
            // If worn on head by the action taker then remove the securityhelmet.SecliteEntity action via RemoveAction.
            if (component.Wearer != null && component.LightEntity != null)
            {
                _actionsSystem.RemoveAction(component.Wearer.Value, component.LightEntity);
            }
        }
        private void OnLightInserted(EntityUid uid, HandheldLightItemSlotComponent component, EntInsertedIntoContainerMessage args)
        {
            // If worn on head by the action taker add the lightcomponent.toggleaction via addAction.
            if (component.Wearer != null && TryComp(args.Entity, out HandheldLightComponent? lightComponent) && component.LightEntity != null)
            {
                _actionsSystem.AddAction(component.Wearer.Value, ref component.LightEntity, lightComponent.ToggleAction, args.Entity);
            }
        }
        private void OnGotEquipped(EntityUid uid, HandheldLightItemSlotComponent component, GotEquippedEvent args)
        {
            var lightOption = _itemSlotsSystem.GetItemOrNull(uid, "light");
            component.Wearer = args.Equipee;

            if (lightOption != null &&
              TryComp(lightOption, out HandheldLightComponent? lightComponent))
            {
                _actionsSystem.AddAction(args.Equipee, ref component.LightEntity, lightComponent.ToggleAction, lightOption.Value);
            }
        }

        private void OnGotUnequipped(EntityUid uid, HandheldLightItemSlotComponent component, GotUnequippedEvent args)
        {
            var lightOption = _itemSlotsSystem.GetItemOrNull(uid, "light");
            component.Wearer = null;

            if (lightOption == null || component.LightEntity == null)
                return;

            _actionsSystem.RemoveAction(args.Equipee, component.LightEntity);
        }

    }
}
