using Content.Shared._Funkystation.Atmos.Components;
using Content.Server._Funkystation.Atmos.Components;
using Content.Server.Power.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Robust.Server.GameObjects;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Shared.Atmos;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Atmos.Prototypes;
using Robust.Shared.Configuration;
using System.Numerics;
using Robust.Shared.Map;
using Content.Server.Maps;
using Robust.Shared.Map.Components;

namespace Content.Server._Funkystation.Atmos.Systems
{
    public sealed class CrystallizerSystem : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly SharedMapSystem _map = default!;

        private const float MinProgressAmount = 3f;
        private const float MinDeviationRate = 0.90f;
        private const float MaxDeviationRate = 1.10f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CrystallizerComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
            SubscribeLocalEvent<CrystallizerComponent, CrystallizerToggleMessage>(OnToggleMessage);
            SubscribeLocalEvent<CrystallizerComponent, CrystallizerSelectRecipeMessage>(OnSelectRecipeMessage);
            SubscribeLocalEvent<CrystallizerComponent, CrystallizerSetGasInputMessage>(OnSetGasInputMessage);
            SubscribeLocalEvent<CrystallizerComponent, AtmosDeviceUpdateEvent>(OnDeviceAtmosUpdate);
        }

        private void OnBeforeOpened(Entity<CrystallizerComponent> ent, ref BeforeActivatableUIOpenEvent args)
        {
            DirtyUI(ent, ent.Comp);
        }

        private void OnToggleMessage(EntityUid uid, CrystallizerComponent crystallizer, CrystallizerToggleMessage args)
        {
            var powerState = _power.TogglePower(uid);
            DirtyUI(uid, crystallizer);
        }

        private void OnSelectRecipeMessage(EntityUid uid, CrystallizerComponent crystallizer, CrystallizerSelectRecipeMessage args)
        {
            if (args.RecipeId == crystallizer.SelectedRecipeId) return;
            if (GetInputPipeMixture(uid, crystallizer, out var pipeMix) && pipeMix != null)
            {
                _atmos.Merge(pipeMix, crystallizer.CrystallizerGasMixture);
                crystallizer.CrystallizerGasMixture = new();
            }
            crystallizer.SelectedRecipeId = args.RecipeId;
            crystallizer.ProgressBar = 0f;
            crystallizer.QualityLoss = 0f;
            crystallizer.TotalRecipeMoles = CalculateTotalRecipeMoles(crystallizer);
            UpdateProgressBarUI(uid, crystallizer);
            DirtyUI(uid, crystallizer);
        }

        private void OnSetGasInputMessage(EntityUid uid, CrystallizerComponent crystallizer, CrystallizerSetGasInputMessage args)
        {
            crystallizer.GasInput = Math.Clamp(args.GasInput, 0f, 250f);
            DirtyUI(uid, crystallizer);
        }

        private void OnDeviceAtmosUpdate(EntityUid uid, CrystallizerComponent crystallizer, ref AtmosDeviceUpdateEvent args)
        {
            if (!_power.IsPowered(uid) || !TryComp<ApcPowerReceiverComponent>(uid, out _))
                return;

            ProcessGasInput(uid, crystallizer);
            ProcessRecipe(uid, crystallizer);
            ProcessTemperatureRegulator(uid, crystallizer, args.dt);
            UpdateGasMixtureUI(uid, crystallizer);
        }

        private void DirtyUI(EntityUid uid, CrystallizerComponent? crystallizer, UserInterfaceComponent? ui = null)
        {
            if (!Resolve(uid, ref crystallizer, ref ui, false))
                return;

            ApcPowerReceiverComponent? powerReceiver = null;
            if (!Resolve(uid, ref powerReceiver))
                return;

            _userInterfaceSystem.SetUiState(uid, CrystallizerUiKey.Key,
                new CrystallizerBoundUserInterfaceState(
                    !powerReceiver.PowerDisabled,
                    crystallizer.SelectedRecipeId,
                    crystallizer.GasInput,
                    crystallizer.CrystallizerGasMixture,
                    crystallizer.ProgressBar,
                    crystallizer.QualityLoss));
        }

        private void UpdateGasMixtureUI(EntityUid uid, CrystallizerComponent? crystallizer, UserInterfaceComponent? ui = null)
        {
            if (!Resolve(uid, ref crystallizer, ref ui, false))
                return;

            _userInterfaceSystem.ServerSendUiMessage(uid, CrystallizerUiKey.Key,
                new CrystallizerUpdateGasMixtureMessage(crystallizer.CrystallizerGasMixture));
        }

        private void UpdateProgressBarUI(EntityUid uid, CrystallizerComponent? crystallizer, UserInterfaceComponent? ui = null)
        {
            if (!Resolve(uid, ref crystallizer, ref ui, false))
                return;

            _userInterfaceSystem.ServerSendUiMessage(uid, CrystallizerUiKey.Key,
                new CrystallizerProgressBarMessage(crystallizer.ProgressBar));
        }

        private void ProcessGasInput(EntityUid uid, CrystallizerComponent crystallizer)
        {
            var crystalMix = crystallizer.CrystallizerGasMixture;
            if (GetInputPipeMixture(uid, crystallizer, out var pipeMix) && pipeMix != null)
            {
                if (string.IsNullOrEmpty(crystallizer.SelectedRecipeId) ||
                    !_prototypeManager.TryIndex<CrystallizerRecipePrototype>(crystallizer.SelectedRecipeId, out var recipe))
                    return;

                float recipeTotalMoles = 0;
                for (int gasIndex = 0; gasIndex < recipe.MinimumRequirements.Length; gasIndex++)
                {
                    if (recipe.MinimumRequirements[gasIndex] > 0 && crystalMix.GetMoles(gasIndex) < recipe.MinimumRequirements[gasIndex] * 2)
                    {
                        recipeTotalMoles += pipeMix.GetMoles(gasIndex);
                    }
                }

                if (recipeTotalMoles <= 0)
                    return;

                var newMix = new GasMixture();
                bool anyGasRemoved = false;

                newMix.Temperature = pipeMix.Temperature;

                for (int gasIndex = 0; gasIndex < recipe.MinimumRequirements.Length; gasIndex++)
                {
                    if (recipe.MinimumRequirements[gasIndex] > 0)
                    {
                        float requiredMoles = recipe.MinimumRequirements[gasIndex];
                        if (crystalMix.GetMoles(gasIndex) >= requiredMoles * 2)
                            continue;

                        float availableMoles = pipeMix.GetMoles(gasIndex);
                        float proportion = availableMoles / recipeTotalMoles;
                        proportion = Math.Min(proportion, 1.0f);

                        float molesToRemove = crystallizer.GasInput * 0.5f * proportion;

                        float molesRemoved = Math.Min(availableMoles, molesToRemove);
                        if (molesRemoved > 0)
                        {
                            pipeMix.AdjustMoles(gasIndex, -molesRemoved);
                            newMix.AdjustMoles(gasIndex, molesRemoved);
                            anyGasRemoved = true;
                        }
                    }
                }

                if (anyGasRemoved)
                {
                    _atmos.Merge(crystalMix, newMix);
                }
            }
        }

        private void ProcessRecipe(EntityUid uid, CrystallizerComponent crystallizer)
        {
            var crystalMix = crystallizer.CrystallizerGasMixture;
            if (string.IsNullOrEmpty(crystallizer.SelectedRecipeId) ||
                !_prototypeManager.TryIndex<CrystallizerRecipePrototype>(crystallizer.SelectedRecipeId, out var recipe) ||
                crystalMix.TotalMoles <= 0)
                return;

            bool gasCheck = CheckGasRequirements(crystalMix, recipe);
            bool tempCheck = CheckTempRequirements(crystalMix, recipe);

            if (gasCheck && tempCheck)
            {
                HeatCalculations(crystalMix, recipe, crystallizer);
                float progressFactor = 5f / MathF.Max(MathF.Log10(crystallizer.TotalRecipeMoles * 0.1f), 0.01f);
                crystallizer.ProgressBar = Math.Min(crystallizer.ProgressBar + MinProgressAmount * progressFactor, 100f);
            }
            else
            {
                crystallizer.QualityLoss = Math.Min(crystallizer.QualityLoss + 0.5f, 100f);
                crystallizer.ProgressBar = Math.Max(crystallizer.ProgressBar - 1f, 0f);
            }

            if (crystallizer.ProgressBar >= 100f)
            {
                CompleteRecipe(uid, crystallizer, recipe);
            }
            UpdateProgressBarUI(uid, crystallizer);
        }

        private bool CheckGasRequirements(GasMixture crystalMix, CrystallizerRecipePrototype recipe)
        {
            for (int gasIndex = 0; gasIndex < recipe.MinimumRequirements.Length; gasIndex++)
            {
                if (recipe.MinimumRequirements[gasIndex] > 0)
                {
                    float availableMoles = crystalMix.GetMoles(gasIndex);
                    if (availableMoles < recipe.MinimumRequirements[gasIndex])
                        return false;
                }
            }
            return true;
        }

        private bool CheckTempRequirements(GasMixture crystalMix, CrystallizerRecipePrototype recipe)
        {
            float temperature = crystalMix.Temperature;
            return temperature >= recipe.MinimumTemperature * MinDeviationRate &&
                   temperature <= recipe.MaximumTemperature * MaxDeviationRate;
        }

        private void HeatCalculations(GasMixture crystalMix, CrystallizerRecipePrototype recipe, CrystallizerComponent crystallizer)
        {
            float progressAmountToQuality = MinProgressAmount * 4.5f / MathF.Max(MathF.Log10(crystallizer.TotalRecipeMoles * 0.1f), 0.01f);
            float medianTemperature = (recipe.MaximumTemperature + recipe.MinimumTemperature) / 2f;

            if ((crystalMix.Temperature >= recipe.MinimumTemperature * MinDeviationRate && crystalMix.Temperature <= recipe.MinimumTemperature) ||
                (crystalMix.Temperature >= recipe.MaximumTemperature && crystalMix.Temperature <= recipe.MaximumTemperature * MaxDeviationRate))
            {
                crystallizer.QualityLoss = Math.Min(crystallizer.QualityLoss + progressAmountToQuality, 100f);
            }

            if (crystalMix.Temperature >= medianTemperature * MinDeviationRate && crystalMix.Temperature <= medianTemperature * MaxDeviationRate)
            {
                crystallizer.QualityLoss = Math.Max(crystallizer.QualityLoss - progressAmountToQuality, -85f);
            }

            float heatCapacity = _atmos.GetHeatCapacity(crystalMix, true);
            if (heatCapacity > 0)
            {
                crystalMix.Temperature = Math.Max(crystalMix.Temperature + recipe.EnergyRelease / heatCapacity, Atmospherics.TCMB);
            }
        }

        private void CompleteRecipe(EntityUid uid, CrystallizerComponent crystallizer, CrystallizerRecipePrototype recipe)
        {
            var crystalMix = crystallizer.CrystallizerGasMixture;

            // Consume gases
            for (int gasIndex = 0; gasIndex < recipe.MinimumRequirements.Length; gasIndex++)
            {
                if (recipe.MinimumRequirements[gasIndex] > 0)
                {
                    float requiredMoles = recipe.MinimumRequirements[gasIndex];
                    float amountConsumed = requiredMoles + (requiredMoles * crystallizer.QualityLoss * 0.01f);
                    float availableMoles = crystalMix.GetMoles(gasIndex);
                    if (availableMoles < amountConsumed)
                    {
                        crystallizer.QualityLoss = Math.Min(crystallizer.QualityLoss + 10f, 100f);
                    }
                    crystalMix.AdjustMoles(gasIndex, -Math.Min(availableMoles, amountConsumed));
                }
            }

            // Spawn products
            var mapCoords = _transform.GetMapCoordinates(uid);
            if (TryComp<TransformComponent>(uid, out var transform) && transform.MapID != MapId.Nullspace)
            {
                var gridUid = transform.GridUid;
                if (gridUid != null && EntityManager.TryGetComponent<MapGridComponent>(gridUid, out var mapGrid))
                {
                    var tile = _map.CoordinatesToTile(gridUid.Value, mapGrid, mapCoords);
                    var southTile = tile + new Vector2i(0, -1);
                    var coords = _map.GridTileToLocal(gridUid.Value, mapGrid, southTile);
                    foreach (var (productId, amount) in recipe.Products)
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            var entity = EntityManager.SpawnEntity(productId, coords);
                        }
                    }
                }
            }

            // Reset
            crystallizer.ProgressBar = 0f;
            crystallizer.QualityLoss = 0f;
        }

        private float CalculateTotalRecipeMoles(CrystallizerComponent crystallizer)
        {
            if (string.IsNullOrEmpty(crystallizer.SelectedRecipeId) ||
                !_prototypeManager.TryIndex<CrystallizerRecipePrototype>(crystallizer.SelectedRecipeId, out var recipe))
                return 0f;

            float total = 0f;
            foreach (var moles in recipe.MinimumRequirements)
            {
                total += moles;
            }
            return total;
        }

        private void ProcessTemperatureRegulator(EntityUid uid, CrystallizerComponent crystallizer, float dt)
        {
            var crystalMix = crystallizer.CrystallizerGasMixture;
            if (!GetRegulatorPipeMixture(uid, crystallizer, out var regulatorMix) || regulatorMix == null)
                return;

            if (crystalMix.TotalMoles <= 0)
                return;

            float regulatorHeatCapacity = _atmos.GetHeatCapacity(regulatorMix, true);
            float crystalHeatCapacity = _atmos.GetHeatCapacity(crystalMix, true);

            if (regulatorHeatCapacity < Atmospherics.MinimumHeatCapacity || crystalHeatCapacity < Atmospherics.MinimumHeatCapacity)
                return;

            float temperatureDelta = regulatorMix.Temperature - crystalMix.Temperature;

            const float conductivityRatio = 0.95f;
            float heatAmount = conductivityRatio * temperatureDelta * (regulatorHeatCapacity * crystalHeatCapacity / (regulatorHeatCapacity + crystalHeatCapacity));

            regulatorMix.Temperature = Math.Max(regulatorMix.Temperature - heatAmount / regulatorHeatCapacity, Atmospherics.TCMB);
            crystalMix.Temperature = Math.Max(crystalMix.Temperature + heatAmount / crystalHeatCapacity, Atmospherics.TCMB);
        }

        private bool GetInputPipeMixture(EntityUid uid, CrystallizerComponent crystallizer, out GasMixture? mixture)
        {
            mixture = null;
            if (!_nodeContainer.TryGetNode(uid, crystallizer.InletName, out PipeNode? inlet))
                return false;

            mixture = inlet.Air;
            return true;
        }

        private bool GetRegulatorPipeMixture(EntityUid uid, CrystallizerComponent crystallizer, out GasMixture? mixture)
        {
            mixture = null;
            if (!_nodeContainer.TryGetNode(uid, crystallizer.RegulatorName, out PipeNode? regulator))
                return false;

            mixture = regulator.Air;
            return true;
        }
    }
}
