using System.Linq;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Content.Shared._NF.Interaction.Components; // Frontier

namespace Content.Server.Silicons.Borgs;

/// <inheritdoc/>
public sealed partial class BorgSystem
{
    public void InitializeModules()
    {
        SubscribeLocalEvent<BorgModuleComponent, EntGotInsertedIntoContainerMessage>(OnModuleGotInserted);
        SubscribeLocalEvent<BorgModuleComponent, EntGotRemovedFromContainerMessage>(OnModuleGotRemoved);

        SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleInstalledEvent>(OnSelectableInstalled);
        SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleUninstalledEvent>(OnSelectableUninstalled);
        SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleActionSelectedEvent>(OnSelectableAction);

        SubscribeLocalEvent<ItemBorgModuleComponent, ComponentStartup>(OnProvideItemStartup);
        SubscribeLocalEvent<ItemBorgModuleComponent, BorgModuleSelectedEvent>(OnItemModuleSelected);
        SubscribeLocalEvent<ItemBorgModuleComponent, BorgModuleUnselectedEvent>(OnItemModuleUnselected);
    }

    private void OnModuleGotInserted(EntityUid uid, BorgModuleComponent component, EntGotInsertedIntoContainerMessage args)
    {
        var chassis = args.Container.Owner;

        if (!TryComp<BorgChassisComponent>(chassis, out var chassisComp) ||
            args.Container != chassisComp.ModuleContainer ||
            !Toggle.IsActivated(chassis))
            return;

        if (!_powerCell.HasDrawCharge(uid))
            return;

        InstallModule(chassis, uid, chassisComp, component);
    }

    private void OnModuleGotRemoved(EntityUid uid, BorgModuleComponent component, EntGotRemovedFromContainerMessage args)
    {
        var chassis = args.Container.Owner;

        if (!TryComp<BorgChassisComponent>(chassis, out var chassisComp) ||
            args.Container != chassisComp.ModuleContainer)
            return;

        UninstallModule(chassis, uid, chassisComp, component);
    }

    private void OnProvideItemStartup(EntityUid uid, ItemBorgModuleComponent component, ComponentStartup args)
    {
        component.ProvidedContainer = Container.EnsureContainer<Container>(uid, component.ProvidedContainerId);
    }

    private void OnSelectableInstalled(EntityUid uid, SelectableBorgModuleComponent component, ref BorgModuleInstalledEvent args)
    {
        var chassis = args.ChassisEnt;

        if (_actions.AddAction(chassis, ref component.ModuleSwapActionEntity, out var action, component.ModuleSwapActionId, uid))
        {
            if(TryComp<BorgModuleIconComponent>(uid, out var moduleIconComp))
            {
                action.Icon = moduleIconComp.Icon;
            };
            action.EntityIcon = uid;
            Dirty(component.ModuleSwapActionEntity.Value, action);
        }

        if (!TryComp(chassis, out BorgChassisComponent? chassisComp))
            return;

        if (chassisComp.SelectedModule == null)
            SelectModule(chassis, uid, chassisComp, component);
    }

    private void OnSelectableUninstalled(EntityUid uid, SelectableBorgModuleComponent component, ref BorgModuleUninstalledEvent args)
    {
        var chassis = args.ChassisEnt;
        _actions.RemoveProvidedActions(chassis, uid);
        if (!TryComp(chassis, out BorgChassisComponent? chassisComp))
            return;

        if (chassisComp.SelectedModule == uid)
            UnselectModule(chassis, chassisComp);
    }

    private void OnSelectableAction(EntityUid uid, SelectableBorgModuleComponent component, BorgModuleActionSelectedEvent args)
    {
        var chassis = args.Performer;
        if (!TryComp<BorgChassisComponent>(chassis, out var chassisComp))
            return;

        var selected = chassisComp.SelectedModule;

        args.Handled = true;
        UnselectModule(chassis, chassisComp);

        if (selected != uid)
        {
            SelectModule(chassis, uid, chassisComp, component);
        }
    }

    /// <summary>
    /// Selects a module, enabling the borg to use its provided abilities.
    /// </summary>
    public void SelectModule(EntityUid chassis,
        EntityUid moduleUid,
        BorgChassisComponent? chassisComp = null,
        SelectableBorgModuleComponent? selectable = null,
        BorgModuleComponent? moduleComp = null)
    {
        if (LifeStage(chassis) >= EntityLifeStage.Terminating)
            return;

        if (!Resolve(chassis, ref chassisComp))
            return;

        if (!Resolve(moduleUid, ref moduleComp) || !moduleComp.Installed || moduleComp.InstalledEntity != chassis)
        {
            Log.Error($"{ToPrettyString(chassis)} attempted to select uninstalled module {ToPrettyString(moduleUid)}");
            return;
        }

        if (selectable == null && !HasComp<SelectableBorgModuleComponent>(moduleUid))
        {
            Log.Error($"{ToPrettyString(chassis)} attempted to select invalid module {ToPrettyString(moduleUid)}");
            return;
        }

        if (!chassisComp.ModuleContainer.Contains(moduleUid))
        {
            Log.Error($"{ToPrettyString(chassis)} does not contain the installed module {ToPrettyString(moduleUid)}");
            return;
        }

        if (chassisComp.SelectedModule != null)
            return;

        if (chassisComp.SelectedModule == moduleUid)
            return;

        UnselectModule(chassis, chassisComp);

        var ev = new BorgModuleSelectedEvent(chassis);
        RaiseLocalEvent(moduleUid, ref ev);
        chassisComp.SelectedModule = moduleUid;
        Dirty(chassis, chassisComp);
    }

    /// <summary>
    /// Unselects a module, removing its provided abilities
    /// </summary>
    public void UnselectModule(EntityUid chassis, BorgChassisComponent? chassisComp = null)
    {
        if (LifeStage(chassis) >= EntityLifeStage.Terminating)
            return;

        if (!Resolve(chassis, ref chassisComp))
            return;

        if (chassisComp.SelectedModule == null)
            return;

        var ev = new BorgModuleUnselectedEvent(chassis);
        RaiseLocalEvent(chassisComp.SelectedModule.Value, ref ev);
        chassisComp.SelectedModule = null;
        Dirty(chassis, chassisComp);
    }

    private void OnItemModuleSelected(EntityUid uid, ItemBorgModuleComponent component, ref BorgModuleSelectedEvent args)
    {
        ProvideItems(args.Chassis, uid, component: component);
    }

    private void OnItemModuleUnselected(EntityUid uid, ItemBorgModuleComponent component, ref BorgModuleUnselectedEvent args)
    {
        RemoveProvidedItems(args.Chassis, uid, component: component);
    }

    private void ProvideItems(EntityUid chassis, EntityUid uid, BorgChassisComponent? chassisComponent = null, ItemBorgModuleComponent? component = null)
    {
        if (!Resolve(chassis, ref chassisComponent) || !Resolve(uid, ref component))
            return;

        if (!TryComp<HandsComponent>(chassis, out var hands))
            return;

        var xform = Transform(chassis);
        foreach (var itemProto in component.Items)
        {
            EntityUid item;

            if (!component.ItemsCreated)
            {
                item = Spawn(itemProto, xform.Coordinates);
            }
            else
            {
                item = component.ProvidedContainer.ContainedEntities
                    .FirstOrDefault(ent => Prototype(ent)?.ID == itemProto);
                if (!item.IsValid())
                {
                    Log.Debug($"no items found: {component.ProvidedContainer.ContainedEntities.Count}");
                    continue;
                }

                _container.Remove(item, component.ProvidedContainer, force: true);
            }

            if (!item.IsValid())
            {
                Log.Debug("no valid item");
                continue;
            }

            var handId = $"{uid}-item{component.HandCounter}";
            component.HandCounter++;
            _hands.AddHand(chassis, handId, HandLocation.Middle, hands);
            _hands.DoPickup(chassis, hands.Hands[handId], item, hands);
            EnsureComp<UnremoveableComponent>(item);
            component.ProvidedItems.Add(handId, item);
        }

        // Frontier: droppable cyborg items
        foreach (var itemProto in component.DroppableItems)
        {
            EntityUid item;

            if (!component.ItemsCreated)
            {
                item = Spawn(itemProto.ID, xform.Coordinates);
                var placeComp = EnsureComp<HandPlaceholderRemoveableComponent>(item);
                placeComp.Whitelist = itemProto.Whitelist;
                placeComp.Prototype = itemProto.ID;
                Dirty(item, placeComp);
            }
            else
            {
                item = component.ProvidedContainer.ContainedEntities
                    .FirstOrDefault(ent => _whitelistSystem.IsWhitelistPassOrNull(itemProto.Whitelist, ent) || TryComp<HandPlaceholderComponent>(ent, out var placeholder));
                if (!item.IsValid())
                {
                    Log.Debug($"no items found: {component.ProvidedContainer.ContainedEntities.Count}");
                    continue;
                }

                // Just in case, make sure the borg can't drop the placeholder.
                if (!HasComp<HandPlaceholderComponent>(item))
                {
                    var placeComp = EnsureComp<HandPlaceholderRemoveableComponent>(item);
                    placeComp.Whitelist = itemProto.Whitelist;
                    placeComp.Prototype = itemProto.ID;
                    Dirty(item, placeComp);
                }
            }

            if (!item.IsValid())
            {
                Log.Debug("no valid item");
                continue;
            }

            var handId = $"{uid}-item{component.HandCounter}";
            component.HandCounter++;
            _hands.AddHand(chassis, handId, HandLocation.Middle, hands);
            _hands.DoPickup(chassis, hands.Hands[handId], item, hands);
            if (hands.Hands[handId].HeldEntity != item)
            {
                // If we didn't pick up our expected item, delete the hand.  No free hands!
                _hands.RemoveHand(chassis, handId);
            }
            else if (HasComp<HandPlaceholderComponent>(item))
            {
                // Placeholders can't be put down, must be changed after picked up (otherwise it'll fail to pick up)
                EnsureComp<UnremoveableComponent>(item);
            }
            component.DroppableProvidedItems.Add(handId, (item, itemProto));
        }
        // End Frontier: droppable cyborg items

        component.ItemsCreated = true;
    }

    private void RemoveProvidedItems(EntityUid chassis, EntityUid uid, BorgChassisComponent? chassisComponent = null, ItemBorgModuleComponent? component = null)
    {
        if (!Resolve(chassis, ref chassisComponent) || !Resolve(uid, ref component))
            return;

        if (!TryComp<HandsComponent>(chassis, out var hands))
            return;

        if (TerminatingOrDeleted(uid))
        {
            foreach (var (hand, item) in component.ProvidedItems)
            {
                QueueDel(item);
                _hands.RemoveHand(chassis, hand, hands);
            }
            component.ProvidedItems.Clear();
            // Frontier: droppable items
            foreach (var (hand, item) in component.DroppableProvidedItems)
            {
                QueueDel(item.Item1);
                _hands.RemoveHand(chassis, hand, hands);
            }
            component.DroppableProvidedItems.Clear();
            // End Frontier: droppable items
            return;
        }

        foreach (var (handId, item) in component.ProvidedItems)
        {
            if (LifeStage(item) <= EntityLifeStage.MapInitialized)
            {
                RemComp<UnremoveableComponent>(item);
                _container.Insert(item, component.ProvidedContainer);
            }
            _hands.RemoveHand(chassis, handId, hands);
        }
        component.ProvidedItems.Clear();
        // Frontier: remove all items from borg hands directly, not from the provided items set
        foreach (var (handId, _) in component.DroppableProvidedItems)
        {
            _hands.TryGetHand(chassis, handId, out var hand, hands);
            if (hand?.HeldEntity != null)
            {
                RemComp<UnremoveableComponent>(hand.HeldEntity.Value);
                _container.Insert(hand.HeldEntity.Value, component.ProvidedContainer);
            }

            _hands.RemoveHand(chassis, handId, hands);
        }
        component.DroppableProvidedItems.Clear();
        // End Frontier
    }

    /// <summary>
    /// Checks if a given module can be inserted into a borg
    /// </summary>
    public bool CanInsertModule(EntityUid uid, EntityUid module, BorgChassisComponent? component = null, BorgModuleComponent? moduleComponent = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(module, ref moduleComponent))
            return false;

        if (component.ModuleContainer.ContainedEntities.Count >= component.MaxModules)
        {
            if (user != null)
                Popup.PopupEntity(Loc.GetString("borg-module-too-many"), uid, user.Value);
            return false;
        }

        if (_whitelistSystem.IsWhitelistFail(component.ModuleWhitelist, module))
        {
            if (user != null)
                Popup.PopupEntity(Loc.GetString("borg-module-whitelist-deny"), uid, user.Value);
            return false;
        }

        if (TryComp<ItemBorgModuleComponent>(module, out var itemModuleComp))
        {
            var droppableComparer = new DroppableBorgItemComparer(); // Frontier: cached comparer
            foreach (var containedModuleUid in component.ModuleContainer.ContainedEntities)
            {
                if (!TryComp<ItemBorgModuleComponent>(containedModuleUid, out var containedItemModuleComp))
                    continue;

                if (containedItemModuleComp.Items.Count == itemModuleComp.Items.Count &&
                    containedItemModuleComp.DroppableItems.Count == itemModuleComp.DroppableItems.Count && // Frontier
                    containedItemModuleComp.Items.All(itemModuleComp.Items.Contains) &&
                    containedItemModuleComp.DroppableItems.All(x => itemModuleComp.DroppableItems.Contains(x, droppableComparer))) // Frontier
                {
                    if (user != null)
                        Popup.PopupEntity(Loc.GetString("borg-module-duplicate"), uid, user.Value);
                    return false;
                }
            }
        }

        return true;
    }

    // Frontier: droppable borg item comparator
    private sealed class DroppableBorgItemComparer : IEqualityComparer<DroppableBorgItem>
    {
        public bool Equals(DroppableBorgItem? x, DroppableBorgItem? y)
        {
            // Same object (or both null)
            if (ReferenceEquals(x, y))
                return true;
            // One-side null
            if (x == null || y == null)
                return false;
            // Otherwise, use EntProtoId of item
            return x.ID == y.ID;
        }

        public int GetHashCode(DroppableBorgItem obj)
        {
            if (obj is null)
                return 0;
            return obj.ID.GetHashCode();
        }
    }
    // End Frontier

    /// <summary>
    /// Check if a module can be removed from a borg.
    /// </summary>
    /// <param name="borg">The borg that the module is being removed from.</param>
    /// <param name="module">The module to remove from the borg.</param>
    /// <param name="user">The user attempting to remove the module.</param>
    /// <returns>True if the module can be removed.</returns>
    public bool CanRemoveModule(
        Entity<BorgChassisComponent> borg,
        Entity<BorgModuleComponent> module,
        EntityUid? user = null)
    {
        if (module.Comp.DefaultModule)
            return false;

        return true;
    }

    /// <summary>
    /// Installs and activates all modules currently inside the borg's module container
    /// </summary>
    public void InstallAllModules(EntityUid uid, BorgChassisComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var query = GetEntityQuery<BorgModuleComponent>();
        foreach (var moduleEnt in new List<EntityUid>(component.ModuleContainer.ContainedEntities))
        {
            if (!query.TryGetComponent(moduleEnt, out var moduleComp))
                continue;

            InstallModule(uid, moduleEnt, component, moduleComp);
        }
    }

    /// <summary>
    /// Deactivates all modules currently inside the borg's module container
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    public void DisableAllModules(EntityUid uid, BorgChassisComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var query = GetEntityQuery<BorgModuleComponent>();
        foreach (var moduleEnt in new List<EntityUid>(component.ModuleContainer.ContainedEntities))
        {
            if (!query.TryGetComponent(moduleEnt, out var moduleComp))
                continue;

            UninstallModule(uid, moduleEnt, component, moduleComp);
        }
    }

    /// <summary>
    /// Installs a single module into a borg.
    /// </summary>
    public void InstallModule(EntityUid uid, EntityUid module, BorgChassisComponent? component, BorgModuleComponent? moduleComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(module, ref moduleComponent))
            return;

        if (moduleComponent.Installed)
            return;

        moduleComponent.InstalledEntity = uid;
        var ev = new BorgModuleInstalledEvent(uid);
        RaiseLocalEvent(module, ref ev);
    }

    /// <summary>
    /// Uninstalls a single module from a borg.
    /// </summary>
    public void UninstallModule(EntityUid uid, EntityUid module, BorgChassisComponent? component, BorgModuleComponent? moduleComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(module, ref moduleComponent))
            return;

        if (!moduleComponent.Installed)
            return;

        moduleComponent.InstalledEntity = null;
        var ev = new BorgModuleUninstalledEvent(uid);
        RaiseLocalEvent(module, ref ev);
    }

    /// <summary>
    /// Sets <see cref="BorgChassisComponent.MaxModules"/>.
    /// </summary>
    /// <param name="ent">The borg to modify.</param>
    /// <param name="maxModules">The new max module count.</param>
    public void SetMaxModules(Entity<BorgChassisComponent> ent, int maxModules)
    {
        ent.Comp.MaxModules = maxModules;
    }

    /// <summary>
    /// Sets <see cref="BorgChassisComponent.ModuleWhitelist"/>.
    /// </summary>
    /// <param name="ent">The borg to modify.</param>
    /// <param name="whitelist">The new module whitelist.</param>
    public void SetModuleWhitelist(Entity<BorgChassisComponent> ent, EntityWhitelist? whitelist)
    {
        ent.Comp.ModuleWhitelist = whitelist;
    }
}
