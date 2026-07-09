// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Chemistry.Components;
using Content.Goobstation.Shared.Chemistry;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Power;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Power.Components;

#region Pirate: chem recipes
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._Pirate.Chemistry;
using Content.Server.Chemistry.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Pirate.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Content.Shared._Pirate.Plumbing.Components; // Pirate: chem plumbing
#endregion

namespace Content.Goobstation.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="EnergyReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class EnergyReagentDispenserSystem : PirateRecipeDispenserSystemBase<EnergyReagentDispenserComponent> // Pirate: chem recipes
    {
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly BatterySystem _battery = default!;
        #region Pirate: chem recipes
        protected override ItemSlotsSystem ItemSlotsSystem => _itemSlotsSystem;
        protected override string RecipeDiskSlotName => SharedEnergyReagentDispenser.RecipeDiskSlotName;
        protected override int RecipeNameMaxLength => SharedEnergyReagentDispenser.RecipeNameMaxLength;
        #endregion
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EnergyReagentDispenserComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EntInsertedIntoContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserToggleValveMessage>(OnToggleValveMessage); // Pirate: chem plumbing
            SubscribeLocalEvent<EnergyReagentDispenserComponent, PowerChangedEvent>(OnPowerChanged);
            RegisterPirateRecipeEvents(); // Pirate: chem recipes

            SubscribeLocalEvent<EnergyReagentDispenserComponent, MapInitEvent>(OnMapInit, before: [typeof(ItemSlotsSystem)]);
        }

        private void UpdateUiState(Entity<EnergyReagentDispenserComponent> reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            var outputContainerInfo = PirateDispenserUiHelper.BuildOutputContainerInfo(outputContainer, _solutionContainerSystem, uid => Name(uid)); // Pirate: chem recipes
            var inventory = GetInventory(reagentDispenser.Comp);
            var batteryCharge = 0f;
            var batteryMaxCharge = 0f;
            var currentReceivingEnergy = 0f;
            var usingBattery = false;
            var idleUse = 0f;
            var hasPower = false;

            if (TryComp<BatteryComponent>(reagentDispenser, out var battery))
            {
                batteryCharge = battery.LastCharge;
                batteryMaxCharge = battery.MaxCharge;
            }

            if (TryComp<ApcPowerReceiverBatteryComponent>(reagentDispenser, out var apcPower))
            {
                currentReceivingEnergy = apcPower.BatteryRechargeRate;
                usingBattery = apcPower.Enabled;
                idleUse = apcPower.IdleLoad;
            }

            if (TryComp<ApcPowerReceiverComponent>(reagentDispenser, out var apc))
                hasPower = apc.Powered;

            var valveOpen = TryComp<PlumbingOutletComponent>(reagentDispenser.Owner, out var plumbingOutlet) && plumbingOutlet.Enabled; // Pirate: chem plumbing

            #region Pirate: chem recipes
            var recipeUiData = PirateChemRecipeUiDataHelper.BuildRecipeUiData(
                reagentDispenser,
                SharedEnergyReagentDispenser.RecipeDiskSlotName,
                _prototypeManager,
                _itemSlotsSystem,
                EntityManager);
            #endregion
            var state = new EnergyReagentDispenserBoundUserInterfaceState(
                outputContainerInfo,
                GetNetEntity(outputContainer),
                inventory,
                reagentDispenser.Comp.DispenseAmount,
                batteryCharge,
                batteryMaxCharge,
                currentReceivingEnergy,
                idleUse,
                usingBattery,
                hasPower,// Pirate: chem recipes
                valveOpen,// Pirate: chem plumbing
                recipeUiData.SavedRecipes,// Pirate: chem recipes
                recipeUiData.HasRecipeDisk,// Pirate: chem recipes
                recipeUiData.DiskRecipes,// Pirate: chem recipes
                recipeUiData.IsRecordingRecipe,// Pirate: chem recipes
                recipeUiData.RecordingReagents// Pirate: chem recipes
            );
            _userInterfaceSystem.SetUiState(reagentDispenser.Owner, EnergyReagentDispenserUiKey.Key, state);
        }

        private List<EnergyReagentInventoryItem> GetInventory(EnergyReagentDispenserComponent comp)
        {
            var inventory = new List<EnergyReagentInventoryItem>();

            foreach (var (reagentId, cost) in comp.Reagents)
            {
                if (!_prototypeManager.TryIndex<ReagentPrototype>(reagentId, out var reagentProto))
                    continue;

                inventory.Add(new EnergyReagentInventoryItem(
                    reagentId,
                    reagentProto.LocalizedName,
                    cost,
                    reagentProto.SubstanceColor
                ));
            }

            inventory.Sort((a, b) => string.Compare(a.ReagentLabel, b.ReagentLabel, StringComparison.Ordinal));
            return inventory;
        }

        private void OnSetDispenseAmountMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.Comp.DispenseAmount = message.EnergyReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
        }

        private void OnPowerChanged(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref PowerChangedEvent args) =>
            UpdateUiState(reagentDispenser);

        private void OnDispenseReagentMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserDispenseReagentMessage message)
        {
            #region Pirate: chem recipes
            var amount = FixedPoint2.New((int)reagentDispenser.Comp.DispenseAmount);
            if (reagentDispenser.Comp.RecordingRecipe != null)
            {
                if (PirateChemRecipeSharedHelper.TryAddRecordedReagent(message.ReagentId, amount, reagentDispenser.Comp.RecordingRecipe))
                {
                    UpdateUiState(reagentDispenser);
                    PlayClickSound(reagentDispenser);
                }

                return;
            }
            #endregion

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true }
                || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            if (!TryComp<BatteryComponent>(reagentDispenser, out var battery))
                return;

            var powerRequired = GetPowerCostForReagent(message.ReagentId, amount.Float(), reagentDispenser.Comp); // Pirate: chem recipes

            if (battery.LastCharge < powerRequired)
            {
                _audioSystem.PlayPvs(reagentDispenser.Comp.PowerSound, reagentDispenser, AudioParams.Default.WithVolume(-2f));
                return;
            }

            var sol = new Solution(message.ReagentId, amount);
            if (!_solutionContainerSystem.TryAddSolution(solution.Value, sol))
                return;

            _battery.SetCharge(reagentDispenser.Owner, battery.LastCharge - powerRequired);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
            UpdateUiState(reagentDispenser);
        }

        private void OnClearContainerSolutionMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainerNullable = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            if (outputContainerNullable is not { Valid: true } outputContainer
                || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer, out var solution, out var soln))
                return;

            var refundedPower = soln.Sum(reagent => GetPowerCostForReagent(reagent.Reagent.Prototype, reagent.Quantity.Float(), reagentDispenser)); // Pirate: chem recipes
            if (refundedPower > 0)
            {
                _battery.TryGetBatteryComponent(reagentDispenser, out var batteryComponent, out _);
                if (batteryComponent != null)
                    _battery.SetCharge(reagentDispenser.Owner, batteryComponent.LastCharge + refundedPower);
            }


            _solutionContainerSystem.RemoveAllSolution(solution.Value);
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
        }


        // Pirate: chem plumbing
        private void OnToggleValveMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserToggleValveMessage message)
        {
            if (!TryComp<PlumbingOutletComponent>(reagentDispenser.Owner, out var plumbingOutlet))
                return;

            plumbingOutlet.Enabled = !plumbingOutlet.Enabled;
            Dirty(reagentDispenser.Owner, plumbingOutlet);
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser);
        }

        private static float GetPowerCostForReagent(string reagentId, float amount, EnergyReagentDispenserComponent comp) // Pirate: chem recipes
        {
            return comp.Reagents.TryGetValue(reagentId, out var cost)
                ? cost * amount
                : float.MaxValue;
        }
        private void OnMapInit(Entity<EnergyReagentDispenserComponent> entity, ref MapInitEvent args)
        {
            EnsureItemSlot(entity.Owner, SharedEnergyReagentDispenser.OutputSlotName, entity.Comp.EnergyBeakerSlot); // Pirate: chem recipes
            EnsureItemSlot(entity.Owner, SharedEnergyReagentDispenser.RecipeDiskSlotName, entity.Comp.RecipeDiskSlot); // Pirate: chem recipes
        }

        #region Pirate: chem recipes
        private void SubscribeUpdateUiState<T>(Entity<EnergyReagentDispenserComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
            if (ev is EntRemovedFromContainerMessage removed &&
                removed.Container.ID == SharedEnergyReagentDispenser.RecipeDiskSlotName)
                PlayClickSound(ent);
        }

        protected override bool TryDispenseRecipe(Entity<EnergyReagentDispenserComponent> reagentDispenser, Dictionary<string, FixedPoint2> recipe)
        {
            if (PirateChemRecipeServerHelper.MergeRecipeIntoRecording(reagentDispenser.Comp.RecordingRecipe, recipe))
                return true;

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out _, out _))
                return false;

            if (!_solutionContainerSystem.TryGetRefillableSolution(outputContainer.Value, out var refillable, out var outputSolution))
                return false;

            var totalRequiredQuantity = recipe.Values.Aggregate(FixedPoint2.Zero, (current, quantity) => current + quantity);
            if (totalRequiredQuantity > outputSolution.AvailableVolume)
                return false;

            if (!TryComp<BatteryComponent>(reagentDispenser, out var battery))
                return false;

            var totalPowerRequired = 0f;
            foreach (var (reagentId, quantity) in recipe)
            {
                if (!reagentDispenser.Comp.Reagents.ContainsKey(reagentId))
                    return false;

                totalPowerRequired += GetPowerCostForReagent(reagentId, quantity.Float(), reagentDispenser.Comp);
            }

            if (battery.LastCharge < totalPowerRequired)
                return false;

            foreach (var (reagentId, quantity) in recipe)
            {
                if (!_solutionContainerSystem.TryAddSolution(refillable.Value, new Solution(reagentId, quantity)))
                    return false;
            }

            _battery.SetCharge(reagentDispenser.Owner, battery.LastCharge - totalPowerRequired);
            return true;
        }

        protected override void UpdateRecipeUiState(Entity<EnergyReagentDispenserComponent> reagentDispenser) => UpdateUiState(reagentDispenser);
        #endregion
    }
}
