using Content.Server.Kitchen.Components;
using Content.Server.Nyanotrasen.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Nyanotrasen.Kitchen.UI;
using Content.Shared.Storage;
using Content.Shared.Tools.Components;

namespace Content.Server.Nyanotrasen.Kitchen.EntitySystems;

public sealed partial class DeepFryerSystem
{
    public bool CanInsertItem(EntityUid uid, DeepFryerComponent component, EntityUid item)
    {
        // Keep this consistent with the checks in TryInsertItem.
        return HasComp<ItemComponent>(item) &&
               !HasComp<StorageComponent>(item) &&
               component.Storage.ContainedEntities.Count < component.StorageMaxEntities;
    }

    private bool TryInsertItem(EntityUid uid, DeepFryerComponent component, EntityUid user, EntityUid item)
    {
        if (!HasComp<ItemComponent>(item))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-interact-using-not-item"),
                uid,
                user);
            return false;
        }

        if (HasComp<StorageComponent>(item))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-storage-no-fit",
                    ("item", item)),
                uid,
                user);
            return false;
        }

        if (component.Storage.ContainedEntities.Count >= component.StorageMaxEntities)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-storage-full"),
                uid,
                user);
            return false;
        }

        if (!_handsSystem.TryDropIntoContainer(user, item, component.Storage))
            return false;

        AfterInsert(uid, component, item);

        _adminLogManager.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(user)} put {ToPrettyString(item)} inside {ToPrettyString(uid)}.");

        return true;
    }

    private void OnInteractUsing(EntityUid uid, DeepFryerComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // By default, allow entities with SolutionTransfer or Tool
        // components to perform their usual actions. Inserting them (if
        // the chef really wants to) will be supported through the UI.
        if (HasComp<SolutionTransferComponent>(args.Used) ||
            HasComp<ToolComponent>(args.Used))
            return;

        if (TryInsertItem(uid, component, args.User, args.Used))
            args.Handled = true;
    }

    private void OnInsertItem(EntityUid uid, DeepFryerComponent component, DeepFryerInsertItemMessage args)
    {
        var user = EntityManager.GetEntity(args.Entity);

        if (!TryComp<HandsComponent>(user, out var handsComponent) ||
            handsComponent.ActiveHandEntity == null)
            return;

        if (handsComponent.ActiveHandEntity != null)
            TryInsertItem(uid, component, user, handsComponent.ActiveHandEntity.Value);
    }
}
