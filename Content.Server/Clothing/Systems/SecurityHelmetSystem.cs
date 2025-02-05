using Content.Shared.Clothing;
using Content.Shared.Actions;
using Content.Shared.Light.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;

namespace Content.Server.GameObjects.Systems
{

    public sealed class SecurityHelmetSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;


        public override void Initialize()
        {
            base.Initialize();

            // Listen for when a player uses an item on the helmet
            SubscribeLocalEvent<SecurityHelmetComponent, GotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<SecurityHelmetComponent, GotUnequippedEvent>(OnGotUnequipped);
            SubscribeLocalEvent<SecurityHelmetComponent, EntRemovedFromContainerMessage>(OnSecliteRemoved);
            SubscribeLocalEvent<SecurityHelmetComponent, EntInsertedIntoContainerMessage>(OnSecliteInserted);

        }
        private void OnSecliteRemoved(EntityUid uid, SecurityHelmetComponent component, EntRemovedFromContainerMessage args)
        {
            // If worn on head by the action taker then remove the securityhelmet.SecliteEntity action via RemoveAction.
            if (component.Wearer != null)
            {
                _actionsSystem.RemoveAction((EntityUid) component.Wearer, component.SecliteEntity);
            }
        }
        private void OnSecliteInserted(EntityUid uid, SecurityHelmetComponent component, EntInsertedIntoContainerMessage args)
        {
            // If worn on head by the action taker add the lightcomponent.toggleaction via addAction.
            if (component.Wearer != null && TryComp(args.Entity, out HandheldLightComponent? lightComponent))
            {
                _actionsSystem.AddAction((EntityUid) component.Wearer, ref component.SecliteEntity, lightComponent.ToggleAction, args.Entity);
            }
        }
        private void OnGotEquipped(EntityUid uid, SecurityHelmetComponent component, GotEquippedEvent args)
        {
            var secliteOption = _itemSlotsSystem.GetItemOrNull(uid, "item");

            if (secliteOption != null &&
              TryComp(secliteOption, out HandheldLightComponent? lightComponent))
            {
                component.Wearer = args.Equipee;
                _actionsSystem.AddAction(args.Equipee, ref component.SecliteEntity, lightComponent.ToggleAction, (EntityUid) secliteOption);
            }
        }

        private void OnGotUnequipped(EntityUid uid, SecurityHelmetComponent component, GotUnequippedEvent args)
        {
            var secliteOption = _itemSlotsSystem.GetItemOrNull(uid, "item");

            if (secliteOption != null)
            {
                component.Wearer = null;
                _actionsSystem.RemoveAction(args.Equipee, component.SecliteEntity);
            }
        }

    }
}
