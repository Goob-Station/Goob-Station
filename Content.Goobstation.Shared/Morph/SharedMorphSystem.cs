using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Polymorph.Components;
using Content.Shared.Polymorph.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Morph;

/// <summary>
/// This handles shared systems for the morph antag
/// </summary>
public abstract class SharedMorphSystem : EntitySystem
{
    [Dependency] private readonly SharedChameleonProjectorSystem _chamleon = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISerializationManager _serMan = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtual = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MorphComponent, AttemptMeleeEvent>(OnAtack);
        SubscribeLocalEvent<ChameleonProjectorComponent, MorphEvent>(TryMorph);
    }

    private void OnAtack(EntityUid uid, MorphComponent component, ref AttemptMeleeEvent args)
    {
        //abort atack if morphed
        if (HasComp<ChameleonDisguisedComponent>(uid))
            args.Cancelled = true;
    }

    private void TryMorph(Entity<ChameleonProjectorComponent> ent, ref MorphEvent arg)
    {
        if (!_chamleon.TryDisguise(ent, arg.Performer, arg.Target))
            return;
        DisguiseInventory(ent, arg.Target);

    }


    /// <summary>
    /// used to mimic target playes inventory
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="target"></param>
    public void DisguiseInventory(Entity<ChameleonProjectorComponent> ent, EntityUid target)
    {
        if(_net.IsClient)
            return;

        var user = ent.Comp.Disguised;

        if (!TryComp<ChameleonDisguisedComponent>(user, out var chamelion))
            return;
        var disguise = chamelion.Disguise;

        if (TryComp<HumanoidAppearanceComponent>(target,out var targetHumanoidAppearance))
        {
            EnsureComp<HumanoidAppearanceComponent>(disguise);
            _humanoidAppearance.CloneAppearance(target, disguise);
        }


        if (TryComp<InventoryComponent>(target, out var inventory))
        {
           //TryCopyComponent<InventoryComponent>(target, disguise,ref inventory,out var inventoryComponent);
           EnsureComp<InventoryComponent>(disguise);

           if (!TryComp<ContainerManagerComponent>(target, out var originalcontainer))
                return;

           EnsureComp<ContainerManagerComponent>(disguise);

            foreach (var container in originalcontainer.Containers)
            {
               _container.EnsureContainer<ContainerSlot>(disguise, container.Value.ID);
            }

            //if (inventory is null)
            //    return;

            foreach (var slot in inventory.Slots)
            {
                if (slot.SlotFlags == SlotFlags.POCKET)// Dont copy pocket slots
                    continue;
                if (!_inventory.TryGetSlotEntity(target, slot.Name, out var slotEntity))
                    continue;

                var name = MetaData(slotEntity.Value).EntityPrototype?.Name;

                if (string.IsNullOrEmpty(name))
                    continue;

               if (!_inventory.SpawnItemInSlot(disguise, slot.Name, name, true, true))
                   continue;

               if (_inventory.TryGetSlotEntity(disguise, slot.Name, out var newItemEntity))
                   EnsureComp<UnremoveableComponent>(newItemEntity.Value);
            }
        }
    }
}
