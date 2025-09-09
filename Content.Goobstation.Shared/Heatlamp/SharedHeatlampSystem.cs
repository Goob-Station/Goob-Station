using Content.Goobstation.Shared.Heatlamp.Upgrades;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Heatlamp;

// TODO: euaghbhdh reorganize
public abstract class SharedHeatlampSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<HeatlampComponent, ItemToggledEvent>(OnItemToggled);
        SubscribeLocalEvent<HeatlampComponent, GotEmaggedEvent>(OnEmagged);

        SubscribeLocalEvent<HeatlampComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<HeatlampComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttempt);
        SubscribeLocalEvent<HeatlampComponent, EntRemovedFromContainerMessage>(OnEntRemovedFromContainer);
    }

    private void OnComponentInit(Entity<HeatlampComponent> ent, ref ComponentInit args)
    {
        // Set up dynamic values
        UpdateDynamicValues(ent);

        // Add our upgrade slots.
        int actualMaximumUpgrades = ent.Comp.MaximumUpgradeCount;
        ent.Comp.MaximumUpgradeCount = 0;
        AddUpgradeSlots(ent, actualMaximumUpgrades);
    }

    private void OnEmagged(Entity<HeatlampComponent> ent, ref GotEmaggedEvent args)
    {
        // Check that we're using an emag
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        // Check that we aren't already emagged
        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        // Update our appearance
        _appearance.SetData(ent, HeatlampVisuals.IsEmagged, true);

        // Do things other than tweaking the appearance, we only do this on the server because the client
        // may raise GotEmaggedEvent multiple times. Normally this is fine, but calling AddUpgradeSlots
        // breaks the heatlamp state on the client.
        if (_net.IsServer)
        {
            // Apply emag damage boost
            ent.Comp.BaseActivatedDamage += ent.Comp.EmagDamageBoost;
            UpdateDynamicValues(ent);

            // Add emag upgrade slots
            AddUpgradeSlots(ent, ent.Comp.EmagUpgradeCountIncrease);

            // Send to client
            Dirty(ent);
        }

        // Tell EmagSystem to use the charge
        args.Handled = true;
    }

    private void OnItemToggled(Entity<HeatlampComponent> ent, ref ItemToggledEvent args)
    {
        // Update our damage if we're on a weapon
        MaybeUpdateDamage(ent);
    }

    private void OnInteractUsing(Entity<HeatlampComponent> ent, ref InteractUsingEvent args)
    {
        // Check that the item is an upgrade and we can accept upgrades
        if (args.Handled
            || !HasComp<HeatlampUpgradeComponent>(args.Used)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots))
            return;

        // Insert the upgrade
        if (_itemSlots.TryInsertEmpty((ent, itemSlots), args.Used, args.User, true))
        {
            // Apply updated upgrade stuff
            UpdateDynamicValues(ent);

            // Send to client
            Dirty(ent);
        }

        args.Handled = true;
    }

    private void OnEntRemovedFromContainer(Entity<HeatlampComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        // Item isn't a battery or something.
        if (!HasComp<HeatlampUpgradeComponent>(args.Entity))
            return;

        // Update upgrade data
        UpdateDynamicValues(ent);

        // Send to client
        Dirty(ent);
    }

    private void OnItemSlotInsertAttempt(Entity<HeatlampComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        // Filter out batteries / other slop
        if (!TryComp<HeatlampUpgradeComponent>(args.Item, out var upgrade))
            return;

        // Check for identical upgrades
        int identicalUpgrades = 0;
        for (int i = 0; i < ent.Comp.MaximumUpgradeCount; i++)
        {
            if (!_itemSlots.TryGetSlot(ent, ItemSlotID(i), out var slot) || slot.Item is not { } item)
                continue;

            if (MetaData(item).EntityPrototype?.ID == MetaData(args.Item).EntityPrototype?.ID)
                identicalUpgrades++;
        }

        // Don't violate the limit
        if (identicalUpgrades >= upgrade.Limit)
            args.Cancelled = true;

        // If we're emagged and installing an illegal upgrade, no we aren't.
        if (upgrade.EmagOnly && !_emag.CheckFlag(ent, EmagType.Interaction))
            args.Cancelled = true;
    }

    /// <summary>
    ///     Updates the modified values for ent, does not apply them to other components.
    /// </summary>
    private void UpdateDynamicValues(Entity<HeatlampComponent> ent)
    {
        // Set up base values
        ent.Comp.ModifiedHeatingPowerDrain = ent.Comp.BaseHeatingPowerDrain;
        ent.Comp.ModifiedMaximumHeatingPerUpdate = ent.Comp.BaseMaximumHeatingPerUpdate;
        ent.Comp.ModifiedCoolingPowerDrain = ent.Comp.BaseCoolingPowerDrain;
        ent.Comp.ModifiedMaximumCoolingPerUpdate = ent.Comp.BaseMaximumCoolingPerUpdate;
        ent.Comp.ModifiedActivatedDamage = new DamageSpecifier(ent.Comp.BaseActivatedDamage);

        // If we don't have any upgrade slots, return.
        if (ent.Comp.MaximumUpgradeCount == 0 || !HasComp<ItemSlotsComponent>(ent))
            return;

        // Iterate over upgrades
        for (int i = 0; i < ent.Comp.MaximumUpgradeCount; i++)
        {
            // Get the slot. This should always exist unless an admin has been fucking around with vv.
            if (!_itemSlots.TryGetSlot(ent, ItemSlotID(i), out var slot))
                continue;

            // Not all slots will have an item in them and the items aren't ordered in any way, you have to run over everything each time.
            if (slot.Item is not { } item)
                continue;

            // Tweak stats with data on upgrade component
            if (TryComp<HeatlampStatsUpgradeComponent>(item, out var statsComp))
            {
                ent.Comp.ModifiedHeatingPowerDrain += statsComp.HeatingPowerDrain;
                ent.Comp.ModifiedMaximumHeatingPerUpdate += statsComp.MaximumHeatingPerUpdate;
                ent.Comp.ModifiedCoolingPowerDrain += statsComp.CoolingPowerDrain;
                ent.Comp.ModifiedMaximumCoolingPerUpdate += statsComp.MaximumCoolingPerUpdate;
                ent.Comp.ModifiedActivatedDamage += statsComp.ActivatedDamage; // TODO: I'm not sure if DamageSpecifiers can be negative. It's probably worth checking and clamping them.
            }

            // TODO: Component upgrades. Maybe use a component field to keep track of added upgrades so they can be removed when a heatlamp upgrade is not present.
        }

        // Clamp upgraded stats to sane values. Heating can't cool you. You can't charge batteries while doing thermal regulation.
        ent.Comp.ModifiedHeatingPowerDrain = float.Max(0, ent.Comp.ModifiedHeatingPowerDrain);
        ent.Comp.ModifiedCoolingPowerDrain = float.Max(0, ent.Comp.ModifiedCoolingPowerDrain);
        ent.Comp.ModifiedMaximumHeatingPerUpdate = float.Max(0, ent.Comp.ModifiedMaximumHeatingPerUpdate);
        ent.Comp.ModifiedMaximumCoolingPerUpdate = float.Max(0, ent.Comp.ModifiedMaximumCoolingPerUpdate);

        // Update damage values on MeleeWeaponComponent
        MaybeUpdateDamage(ent);
    }

    /// <summary>
    ///     Updates damage if attached to a weapon.
    /// </summary>
    private void MaybeUpdateDamage(Entity<HeatlampComponent> ent)
    {
        // Set damage if we're attached to a weapon
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.Damage = new DamageSpecifier(IsActive(ent) ? ent.Comp.ModifiedActivatedDamage : ent.Comp.DeactivatedDamage);
        }
    }

    /// <summary>
    ///     Adds additional upgrade slots to ent.
    /// </summary>
    /// <param name="ent">The heatlamp</param>
    /// <param name="count">Number of slots to add</param>
    private void AddUpgradeSlots(Entity<HeatlampComponent> ent, int count)
    {
        // Tick up the maximum upgrade count
        ent.Comp.MaximumUpgradeCount += count;

        // Add the new upgrade slots
        for (int i = ent.Comp.CurrentUpgradeCount; i < ent.Comp.MaximumUpgradeCount; i++)
        {
            _itemSlots.AddItemSlot(ent, ItemSlotID(i), new ItemSlot
            {
                Whitelist = new EntityWhitelist() { Components = [ "HeatlampUpgrade" ] }
            });
        }

        // Update current upgrade count
        ent.Comp.CurrentUpgradeCount = ent.Comp.MaximumUpgradeCount;
    }

    private bool IsActive(Entity<HeatlampComponent> ent)
    {
        // ReSharper disable once SimplifyConditionalTernaryExpression // that looks like shit
        // Entities with the heatlamp component are not always heatlamps,
        // and will not always have ItemToggleComponents. Namely, the Thermal Regulator
        // organ adds a heatlamp component to it's user.
        return TryComp<ItemToggleComponent>(ent, out var toggle) ? toggle.Activated : true;
    }

    /// <summary>
    ///     Returns the slot id for 0-indexed slot number slot
    /// </summary>
    private string ItemSlotID(int slot) => $"heatlamp-upgrade-{slot}";
}

[Serializable, NetSerializable]
public enum HeatlampVisuals : byte
{
    IsPowered,
    IsEmagged
}
