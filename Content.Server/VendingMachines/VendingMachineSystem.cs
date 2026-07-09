// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack; // Pirate banking
using Content.Server.Store.Components; // Pirate banking
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Server.Vocalization.Systems;
using Content.Shared.Cargo;
using Content.Shared.Damage;
using Content.Shared.Emp;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction; // Pirate banking
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Stacks; // Pirate banking
using Content.Shared._Pirate.Banking; // Pirate banking
using Content.Shared.Throwing;
using Content.Shared.VendingMachines;
using Content.Shared.Wall;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Access.Systems; // Pirate banking
using Content.Server._Pirate.Banking; // Pirate banking
using Content.Shared.Tag; // Pirate banking - IgnoreBalanceChecks

namespace Content.Server.VendingMachines
{
    public sealed class VendingMachineSystem : SharedVendingMachineSystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PricingSystem _pricing = default!;
        [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
        [Dependency] private readonly TagSystem _tag = default!; // Pirate banking - IgnoreBalanceChecks

        // Pirate banking start
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly BankCardSystem _bankCard = default!;
        [Dependency] private readonly IdCardSystem _idCard = default!;
        // Pirate banking end

        private const float WallVendEjectDistanceFromWall = 1f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VendingMachineComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<VendingMachineComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<VendingMachineComponent, PriceCalculationEvent>(OnVendingPrice);
            SubscribeLocalEvent<VendingMachineComponent, TryVocalizeEvent>(OnTryVocalize);

            SubscribeLocalEvent<VendingMachineComponent, VendingMachineSelfDispenseEvent>(OnSelfDispense);

            SubscribeLocalEvent<VendingMachineRestockComponent, PriceCalculationEvent>(OnPriceCalculation);

            // Pirate banking start
            Subs.BuiEvents<VendingMachineComponent>(VendingMachineUiKey.Key, subs =>
            {
                subs.Event<BoundUIOpenedEvent>(OnBoundUIOpened);
            });
            SubscribeLocalEvent<VendingMachineComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<VendingMachineComponent, VendingMachineWithdrawMessage>(OnWithdrawMessage);
            // Pirate banking end
        }

        private void OnVendingPrice(EntityUid uid, VendingMachineComponent component, ref PriceCalculationEvent args)
        {
            var price = 0.0;

            foreach (var entry in component.Inventory.Values)
            {
                if (!PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(uid)} vending.");
                    continue;
                }

                price += entry.Amount * _pricing.GetEstimatedPrice(proto);
            }

            args.Price += price;
        }

        protected override void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args)
        {
            base.OnMapInit(uid, component, args);

            if (HasComp<ApcPowerReceiverComponent>(uid))
            {
                TryUpdateVisualState((uid, component));
            }
        }

        private void OnPowerChanged(EntityUid uid, VendingMachineComponent component, ref PowerChangedEvent args)
        {
            TryUpdateVisualState((uid, component));
        }

        private void OnDamageChanged(EntityUid uid, VendingMachineComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased && component.Broken)
            {
                component.Broken = false;
                Dirty(uid, component);
                TryUpdateVisualState((uid, component));
                return;
            }

            if (component.Broken || component.DispenseOnHitCoolingDown ||
                component.DispenseOnHitChance == null || args.DamageDelta == null)
                return;

            if (args.DamageIncreased && args.DamageDelta.GetTotal() >= component.DispenseOnHitThreshold &&
                _random.Prob(component.DispenseOnHitChance.Value))
            {
                if (component.DispenseOnHitCooldown != null)
                {
                    component.DispenseOnHitEnd = Timing.CurTime + component.DispenseOnHitCooldown.Value;
                }

                EjectRandom(uid, throwItem: true, forceEject: true, component);
            }
        }

        private void OnSelfDispense(EntityUid uid, VendingMachineComponent component, VendingMachineSelfDispenseEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            EjectRandom(uid, throwItem: true, forceEject: false, component);
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.CanShoot"/> property of the vending machine.
        /// </summary>
        public void SetShooting(EntityUid uid, bool canShoot, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanShoot = canShoot;
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.Contraband"/> property of the vending machine.
        /// </summary>
        public void SetContraband(EntityUid uid, bool contraband, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.Contraband = contraband;
            Dirty(uid, component);
        }

        /// <summary>
        /// Ejects a random item from the available stock. Will do nothing if the vending machine is empty.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="throwItem">Whether to throw the item in a random direction after dispensing it.</param>
        /// <param name="forceEject">Whether to skip the regular ejection checks and immediately dispense the item without animation.</param>
        /// <param name="vendComponent"></param>
        public void EjectRandom(EntityUid uid, bool throwItem, bool forceEject = false, VendingMachineComponent? vendComponent = null)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            var availableItems = GetAvailableInventory(uid, vendComponent);
            if (availableItems.Count <= 0)
                return;

            var item = _random.Pick(availableItems);

            if (forceEject)
            {
                vendComponent.NextItemToEject = item.ID;
                vendComponent.ThrowNextItem = throwItem;
                var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
                if (entry != null)
                    entry.Amount--;
                EjectItem(uid, vendComponent, forceEject);
            }
            else
            {
                TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent);
            }
        }

        protected override void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            // No need to update the visual state because we never changed it during a forced eject
            if (!forceEject)
                TryUpdateVisualState((uid, vendComponent));

            if (string.IsNullOrEmpty(vendComponent.NextItemToEject))
            {
                vendComponent.ThrowNextItem = false;
                return;
            }

            // Default spawn coordinates
            var xform = Transform(uid);
            var spawnCoordinates = xform.Coordinates;

            //Make sure the wallvends spawn outside of the wall.
            if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
            {
                var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
                spawnCoordinates = spawnCoordinates.Offset(offset);
            }

            var ent = Spawn(vendComponent.NextItemToEject, spawnCoordinates);

            if (vendComponent.ThrowNextItem)
            {
                var range = vendComponent.NonLimitedEjectRange;
                var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
                _throwingSystem.TryThrow(ent, direction, vendComponent.NonLimitedEjectForce);
            }

            vendComponent.NextItemToEject = null;
            vendComponent.ThrowNextItem = false;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var disabled = EntityQueryEnumerator<EmpDisabledComponent, VendingMachineComponent>();
            while (disabled.MoveNext(out var uid, out _, out var comp))
            {
                if (comp.NextEmpEject < Timing.CurTime)
                {
                    EjectRandom(uid, true, false, comp);
                    comp.NextEmpEject += (5 * comp.EjectDelay);
                }
            }
        }

        private void OnPriceCalculation(EntityUid uid, VendingMachineRestockComponent component, ref PriceCalculationEvent args)
        {
            List<double> priceSets = new();

            // Find the most expensive inventory and use that as the highest price.
            foreach (var vendingInventory in component.CanRestock)
            {
                double total = 0;

                if (PrototypeManager.TryIndex(vendingInventory, out VendingMachineInventoryPrototype? inventoryPrototype))
                {
                    foreach (var (item, amount) in inventoryPrototype.StartingInventory)
                    {
                        if (PrototypeManager.TryIndex(item, out EntityPrototype? entity))
                            total += _pricing.GetEstimatedPrice(entity) * amount;
                    }
                }

                priceSets.Add(total);
            }

            args.Price += priceSets.Max();
        }

        private void OnTryVocalize(Entity<VendingMachineComponent> ent, ref TryVocalizeEvent args)
        {
            args.Cancelled |= ent.Comp.Broken;
        }

        // Pirate banking start
        private void OnBoundUIOpened(EntityUid uid, VendingMachineComponent component, BoundUIOpenedEvent args)
        {
            UpdateVendingMachineInterfaceState(uid, component);
        }

        private void UpdateVendingMachineInterfaceState(EntityUid uid, VendingMachineComponent component)
        {
            var state = new VendingMachineInterfaceState(GetAllInventory(uid, component), GetPriceMultiplier(component),
                component.Credits);

            UISystem.SetUiState(uid, VendingMachineUiKey.Key, state);
        }

        public override void AuthorizedVend(EntityUid uid, EntityUid sender, InventoryType type, string itemId, VendingMachineComponent component)
        {
            if (component.Ejecting)
                return;

            if (!IsAuthorized(uid, sender, component))
                return;

            var entry = GetEntry(uid, itemId, type, component);
            if (entry == null)
            {
                base.AuthorizedVend(uid, sender, type, itemId, component);
                return;
            }

            // Pirate banking start
            if (entry.Amount <= 0)
            {
                Popup.PopupEntity(Loc.GetString("vending-machine-component-try-eject-out-of-stock"), sender, sender);
                Deny((uid, component), sender);
                return;
            }
            // Pirate banking end

            var price = GetPrice(entry, component);
            // Pirate banking - IgnoreBalanceChecks tag bypass
            if (price > 0 && !_tag.HasTag(sender, "IgnoreBalanceChecks"))
            {
                if (component.Credits >= price)
                {
                    component.Credits -= price;
                }
                else if (TryPayWithBankCard(sender, price))
                {
                    Audio.PlayPvs(component.SoundInsertCurrency, uid);
                }
                else
                {
                    Popup.PopupEntity(Loc.GetString("vending-machine-component-no-balance", ("target", uid)), sender, sender);
                    Deny((uid, component), sender);
                    return;
                }
            }

            base.AuthorizedVend(uid, sender, type, itemId, component);
            UpdateVendingMachineInterfaceState(uid, component);
        }

        private bool TryPayWithBankCard(EntityUid user, int amount)
        {
            if (!_idCard.TryFindIdCard(user, out var idCard))
                return false;

            if (!TryComp<BankCardComponent>(idCard.Owner, out var bankCard) || bankCard.AccountId == null)
                return false;

            return _bankCard.TryChangeBalance(bankCard.AccountId.Value, -amount);
        }


        private void OnInteractUsing(EntityUid uid, VendingMachineComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (component.Broken || !this.IsPowered(uid, EntityManager))
                return;

            if (!TryComp<CurrencyComponent>(args.Used, out var currency) ||
                !currency.Price.Keys.Contains(component.CurrencyType))
                return;

            if (!TryComp<StackComponent>(args.Used, out var stack))
                return;

            component.Credits += stack.Count;
            Del(args.Used);
            UpdateVendingMachineInterfaceState(uid, component);
            Audio.PlayPvs(component.SoundInsertCurrency, uid);
            args.Handled = true;
        }

        protected override int GetEntryPrice(EntityPrototype proto)
        {
            var price = (int) _pricing.GetEstimatedPrice(proto);
            return price; 
        }

        private int GetPrice(VendingMachineInventoryEntry entry, VendingMachineComponent comp)
        {
            return (int) (entry.Price * GetPriceMultiplier(comp));
        }

        private double GetPriceMultiplier(VendingMachineComponent comp)
        {
            return comp.PriceMultiplier;
        }

        private void OnWithdrawMessage(EntityUid uid, VendingMachineComponent component, VendingMachineWithdrawMessage args)
        {
            if (component.Credits <= 0)
                return;

            if (!IsAuthorized(uid, args.Actor, component))
                return;
                
            _stackSystem.SpawnAtPosition(component.Credits, PrototypeManager.Index(component.CreditStackPrototype),
                Transform(uid).Coordinates);
            component.Credits = 0;
            Audio.PlayPvs(component.SoundWithdrawCurrency, uid);

            UpdateVendingMachineInterfaceState(uid, component);
        }
        // Pirate banking end
    }
}
