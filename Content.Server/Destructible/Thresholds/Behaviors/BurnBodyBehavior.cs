// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part; // Shitmed Change
using Content.Shared.Body.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Content.Shared.IdentityManagement;
using Content.Server.Body.Systems;

namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public sealed partial class BurnBodyBehavior : IThresholdBehavior
{
    /// <summary>
    ///     The popup displayed upon destruction.
    /// </summary>
    [DataField]
    public LocId PopupMessage = "bodyburn-text-others";

    public void Execute(EntityUid bodyId, DestructibleSystem system, EntityUid? cause = null)
    {
        var transformSystem = system.EntityManager.System<TransformSystem>();
        var inventorySystem = system.EntityManager.System<InventorySystem>();
        var sharedPopupSystem = system.EntityManager.System<SharedPopupSystem>();

        if (system.EntityManager.TryGetComponent<InventoryComponent>(bodyId, out var comp))
        {
            foreach (var item in inventorySystem.GetHandOrInventoryEntities(bodyId))
            {
                transformSystem.DropNextTo(item, bodyId);
            }
        }

        // <Woundmed> BurnBodyBehavior is likely on a BodyPart, need to handle that properly
        if (system.EntityManager.TryGetComponent(bodyId, out BodyPartComponent? partComp)
            && system.EntityManager.System<BodySystem>().TryBurnPart((bodyId, partComp)) is { } burned)
        {
            var identity = Identity.Entity(burned, system.EntityManager);
            sharedPopupSystem.PopupCoordinates(
                Loc.GetString(PopupMessage, ("name", identity)),
                transformSystem.GetMoverCoordinates(bodyId), PopupType.LargeCaution
            );
            return;
        }
        // </Woundmed>

        var bodyIdentity = Identity.Entity(bodyId, system.EntityManager);
        sharedPopupSystem.PopupCoordinates(Loc.GetString(PopupMessage, ("name", bodyIdentity)), transformSystem.GetMoverCoordinates(bodyId), PopupType.LargeCaution);

        //system.EntityManager.QueueDeleteEntity(bodyId);
    }
}
