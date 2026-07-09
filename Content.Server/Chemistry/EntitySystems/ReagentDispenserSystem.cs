// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Diagnostics.CodeAnalysis; // Pirate: chem recipes
using Content.Server._Pirate.Chemistry; // Pirate: chem recipes
using Content.Shared._Pirate.Chemistry; // Pirate: chem recipes
using Content.Shared._Pirate.Plumbing.Components; // Pirate: chem plumbing
using Content.Shared.Chemistry.Reagent; // Pirate: chem recipes
using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Labels.Components;
using Content.Shared.Storage;
using Content.Server.Hands.Systems;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="ReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class ReagentDispenserSystem : PirateRecipeDispenserSystemBase<ReagentDispenserComponent>
    {
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly SolutionTransferSystem _solutionTransferSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly OpenableSystem _openable = default!;
        [Dependency] private readonly HandsSystem _handsSystem = default!;
        #region Pirate: chem recipes
        protected override ItemSlotsSystem ItemSlotsSystem => _itemSlotsSystem;
        protected override string RecipeDiskSlotName => SharedReagentDispenser.RecipeDiskSlotName;
        protected override int RecipeNameMaxLength => SharedReagentDispenser.RecipeNameMaxLength;
        protected override bool AllowRestartRecording => true;
        protected override bool AllowCancelWithoutRecording => true;
        protected override bool ErrorOnSaveFailure => true;
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ReagentDispenserComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, EntInsertedIntoContainerMessage>(SubscribeUpdateUiState, after: [typeof(SharedStorageSystem)]);
            SubscribeLocalEvent<ReagentDispenserComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState, after: [typeof(SharedStorageSystem)]);
            SubscribeLocalEvent<ReagentDispenserComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserEjectContainerMessage>(OnEjectReagentMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserToggleValveMessage>(OnToggleValveMessage); // Pirate: chem plumbing
            RegisterPirateRecipeEvents(); // Pirate: chem recipes

            SubscribeLocalEvent<ReagentDispenserComponent, MapInitEvent>(OnMapInit, before: new[] { typeof(ItemSlotsSystem) });
        }

        private void SubscribeUpdateUiState<T>(Entity<ReagentDispenserComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
            #region Pirate: chem recipes
            if (ev is EntRemovedFromContainerMessage removed &&
                removed.Container.ID == SharedReagentDispenser.RecipeDiskSlotName)
                PlayClickSound(ent);
            #endregion
        }

        private void UpdateUiState(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            var outputContainerInfo = PirateDispenserUiHelper.BuildOutputContainerInfo(outputContainer, _solutionContainerSystem, uid => Name(uid)); // Pirate: chem recipes

            var inventory = GetInventory(reagentDispenser);
            #region Pirate: chem recipes
            var recipeUiData = PirateChemRecipeUiDataHelper.BuildRecipeUiData(
                reagentDispenser,
                SharedReagentDispenser.RecipeDiskSlotName,
                _prototypeManager,
                _itemSlotsSystem,
                EntityManager);

            var valveOpen = TryComp<PlumbingOutletComponent>(reagentDispenser.Owner, out var plumbingOutlet) && plumbingOutlet.Enabled; // Pirate: chem plumbing

            var state = new ReagentDispenserBoundUserInterfaceState(
                outputContainerInfo,
                GetNetEntity(outputContainer),
                inventory,
                reagentDispenser.Comp.DispenseAmount,
                valveOpen, // Pirate: chem plumbing
                recipeUiData.SavedRecipes,
                recipeUiData.HasRecipeDisk,
                recipeUiData.DiskRecipes,
                recipeUiData.IsRecordingRecipe,
                recipeUiData.RecordingReagents);
            #endregion
            _userInterfaceSystem.SetUiState(reagentDispenser.Owner, ReagentDispenserUiKey.Key, state);
        }

        private List<ReagentInventoryItem> GetInventory(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            if (!TryComp<StorageComponent>(reagentDispenser.Owner, out var storage))
            {
                return [];
            }

            var inventory = new List<ReagentInventoryItem>();

            foreach (var (storedContainer, storageLocation) in storage.StoredItems)
            {
                string reagentLabel;
                if (TryComp<LabelComponent>(storedContainer, out var label) && !string.IsNullOrEmpty(label.CurrentLabel))
                    reagentLabel = label.CurrentLabel;
                else
                    reagentLabel = Name(storedContainer);

                // Get volume remaining and color of solution
                FixedPoint2 quantity = 0f;
                var reagentColor = Color.White;
                if (_solutionContainerSystem.TryGetDrainableSolution(storedContainer, out _, out var sol))
                {
                    quantity = sol.Volume;
                    reagentColor = sol.GetColor(_prototypeManager);
                }

                inventory.Add(new ReagentInventoryItem(storageLocation, reagentLabel, quantity, reagentColor));
            }

            return inventory;
        }

        private void OnSetDispenseAmountMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.Comp.DispenseAmount = message.ReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
        }

        private void OnDispenseReagentMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserDispenseReagentMessage message)
        {
            if (!TryComp<StorageComponent>(reagentDispenser.Owner, out var storage))
            {
                return;
            }

            // Ensure that the reagent is something this reagent dispenser can dispense.
            var storageLocation = message.StorageLocation;
            var storedContainer = storage.StoredItems.FirstOrDefault(kvp => kvp.Value == storageLocation).Key;
            if (storedContainer == EntityUid.Invalid)
                return;

            #region Pirate: chem recipes
            if (reagentDispenser.Comp.RecordingRecipe != null)
            {
                var amount = FixedPoint2.New((int)reagentDispenser.Comp.DispenseAmount);
                if (TryRecordDispensedMixture(storedContainer, amount, reagentDispenser.Comp.RecordingRecipe))
                {
                    UpdateUiState(reagentDispenser);
                    PlayClickSound(reagentDispenser);
                }

                return;
            }
            #endregion

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            if (_solutionContainerSystem.TryGetDrainableSolution(storedContainer, out var src, out _) &&
                _solutionContainerSystem.TryGetRefillableSolution(outputContainer.Value, out var dst, out _))
            {
                // force open container, if applicable, to avoid confusing people on why it doesn't dispense
                _openable.SetOpen(storedContainer, true);
                _solutionTransferSystem.Transfer(new SolutionTransferData(reagentDispenser,
                        storedContainer, src.Value,
                        outputContainer.Value, dst.Value,
                        (int)reagentDispenser.Comp.DispenseAmount));
            }

            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
        }

        private void OnEjectReagentMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserEjectContainerMessage message)
        {
            if (!TryComp<StorageComponent>(reagentDispenser.Owner, out var storage))
            {
                return;
            }

            var storageLocation = message.StorageLocation;
            var storedContainer = storage.StoredItems.FirstOrDefault(kvp => kvp.Value == storageLocation).Key;
            if (storedContainer == EntityUid.Invalid)
                return;

            _handsSystem.TryPickupAnyHand(message.Actor, storedContainer);
        }

        private void OnClearContainerSolutionMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            _solutionContainerSystem.RemoveAllSolution(solution.Value);
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem recipes
        }

        #region Pirate: chem plumbing
        private void OnToggleValveMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserToggleValveMessage message)
        {
            if (!TryComp<PlumbingOutletComponent>(reagentDispenser.Owner, out var plumbingOutlet))
                return;

            plumbingOutlet.Enabled = !plumbingOutlet.Enabled;
            Dirty(reagentDispenser.Owner, plumbingOutlet);
            UpdateUiState(reagentDispenser);
            PlayClickSound(reagentDispenser); // Pirate: chem plumbing
        }
        #endregion

        /// <summary>
        /// Initializes the beaker slot
        /// </summary>
        private void OnMapInit(Entity<ReagentDispenserComponent> ent, ref MapInitEvent args)
        {
            EnsureItemSlot(ent.Owner, SharedReagentDispenser.OutputSlotName, ent.Comp.BeakerSlot); // Pirate: chem recipes
            EnsureItemSlot(ent.Owner, SharedReagentDispenser.RecipeDiskSlotName, ent.Comp.RecipeDiskSlot); // Pirate: chem recipes
        }

        #region Pirate: chem recipes
        protected override bool ValidateRecipeForSave(Entity<ReagentDispenserComponent> reagentDispenser, Dictionary<string, FixedPoint2> recipe)
        {
            // Validate that each reagent is currently dispensable by this machine.
            foreach (var reagent in recipe.Keys)
            {
                if (!TryGetStoredContainerForReagentId(reagentDispenser.Owner, reagent, out _))
                    return false;
            }

            return true;
        }

        protected override bool TryDispenseRecipe(Entity<ReagentDispenserComponent> reagentDispenser, Dictionary<string, FixedPoint2> recipe)
        {
            if (PirateChemRecipeServerHelper.MergeRecipeIntoRecording(reagentDispenser.Comp.RecordingRecipe, recipe))
                return true;

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out _, out _))
                return false;

            if (!_solutionContainerSystem.TryGetRefillableSolution(outputContainer.Value, out var dstRefillable, out var dstSolution))
                return false;

            var totalRequiredQuantity = recipe.Values.Aggregate(FixedPoint2.Zero, (current, quantity) => current + quantity);
            if (totalRequiredQuantity > dstSolution.AvailableVolume)
                return false;

            var cachedSources = new Dictionary<string, (EntityUid Container, Entity<Content.Shared.Chemistry.Components.SolutionComponent>? Drainable)>(recipe.Count);

            // Pre-check recipe contents so recipe usage either succeeds as a whole or aborts with a precise error.
            foreach (var (reagentId, quantity) in recipe)
            {
                if (!TryGetStoredContainerForReagentId(reagentDispenser.Owner, reagentId, out var srcContainer))
                    return false;

                if (!_solutionContainerSystem.TryGetDrainableSolution(srcContainer, out var srcDrainable, out var srcSoln))
                    return false;

                var available = srcSoln.GetReagentQuantity(new ReagentId(reagentId, null));
                if (available < quantity)
                    return false;

                // Pirate: chem recipes - recipe quantities are per reagent, so mixed sources are invalid.
                if (srcSoln.Volume - available > FixedPoint2.Zero)
                    return false;

                cachedSources[reagentId] = (srcContainer, srcDrainable);
            }

            foreach (var (reagentId, quantity) in recipe)
            {
                if (!cachedSources.TryGetValue(reagentId, out var source))
                    return false;

                if (source.Drainable is not { } srcDrainable)
                    return false;

                _openable.SetOpen(source.Container, true);
                var transferred = _solutionTransferSystem.Transfer(new SolutionTransferData(
                    reagentDispenser,
                    source.Container,
                    srcDrainable,
                    outputContainer.Value,
                    dstRefillable.Value,
                    quantity));

                if (transferred < quantity)
                    return false;
            }

            return true;
        }

        protected override void UpdateRecipeUiState(Entity<ReagentDispenserComponent> reagentDispenser) => UpdateUiState(reagentDispenser);

        private bool TryRecordDispensedMixture(EntityUid container, FixedPoint2 requestedAmount, Dictionary<string, FixedPoint2> recordingRecipe)
        {
            if (!_solutionContainerSystem.TryGetDrainableSolution(container, out _, out var sol) ||
                sol.Volume <= FixedPoint2.Zero)
            {
                return false;
            }

            var transferredAmount = FixedPoint2.Min(requestedAmount, sol.Volume);
            if (transferredAmount <= FixedPoint2.Zero)
                return false;

            // Pirate: chem recipes - preserve exact dispense amount for pure sources (avoid 10 -> 9 rounding).
            if (sol.Contents.Count == 1 && sol.Contents[0].Reagent.Prototype is { } onlyReagentId)
            {
                return PirateChemRecipeSharedHelper.TryAddRecordedReagent(onlyReagentId, transferredAmount, recordingRecipe);
            }

            var transferRatio = transferredAmount / sol.Volume;
            var recordedAny = false;

            foreach (var reagent in sol.Contents)
            {
                if (reagent.Reagent.Prototype is not { } reagentId)
                    continue;

                var recordedAmount = reagent.Quantity * transferRatio;
                recordedAny |= PirateChemRecipeSharedHelper.TryAddRecordedReagent(reagentId, recordedAmount, recordingRecipe);
            }

            return recordedAny;
        }


        private bool TryGetStoredContainerForReagentId(EntityUid dispenser, string reagentId, [NotNullWhen(true)] out EntityUid container)
        {
            container = EntityUid.Invalid;
            if (!TryComp<StorageComponent>(dispenser, out var storage))
                return false;

            foreach (var (stored, _) in storage.StoredItems)
            {
                if (!_solutionContainerSystem.TryGetDrainableSolution(stored, out _, out var sol))
                    continue;

                if (sol.GetReagentQuantity(new ReagentId(reagentId, null)) <= FixedPoint2.Zero)
                    continue;

                container = stored;
                return true;
            }

            return false;
        }

        #endregion
    }
}
