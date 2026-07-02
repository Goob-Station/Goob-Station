using Content.Server._Pirate.Plumbing.Components;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using SharedAppearanceSystem = Robust.Shared.GameObjects.SharedAppearanceSystem;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles plumbing reactor machine behavior: reactor pulls reagents into a buffer until they reach
///     target quantities, and then fully reacts the buffer and moves products to output container.
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingReactorSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private ChemicalReactionSystem _reactionSystem = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private NodeContainerSystem _nodeContainer = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private PlumbingPullSystem _pullSystem = default!;
    [Dependency] private PowerReceiverSystem _power = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    /// <summary>
    ///     Temperature tolerance for considering target reached (in Kelvin).
    /// </summary>
    private const float TemperatureTolerance = 0.5f;

    /// <summary>
    ///     Minimum allowed target temperature (cosmic microwave background).
    /// </summary>
    private const float MinTemperature = Atmospherics.TCMB;

    /// <summary>
    ///     Maximum allowed target temperature.
    /// </summary>
    private const float MaxTemperature = 1000f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingDeviceUpdateEvent>(OnReactorUpdate);
        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingReactorToggleMessage>(OnToggle);
        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingReactorSetTargetMessage>(OnSetTarget);
        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingReactorRemoveTargetMessage>(OnRemoveTarget);
        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingReactorClearTargetsMessage>(OnClearTargets);
        SubscribeLocalEvent<PlumbingReactorComponent, PlumbingReactorSetTemperatureMessage>(OnSetTemperature);
        SubscribeLocalEvent<PlumbingReactorComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnReactorUpdate(Entity<PlumbingReactorComponent> ent, ref PlumbingDeviceUpdateEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out var bufferEnt, out var buffer))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.OutputSolutionName, out var outputEnt, out var output))
            return;

        if (output.AvailableVolume <= 0)
        {
            UpdateUI(ent);
            return;
        }

        if (ent.Comp.ReagentTargets.Count == 0)
        {
            UpdateUI(ent);
            return;
        }

        if (!_nodeContainer.TryGetNode<PlumbingNode>(ent.Owner, ent.Comp.InletName, out var inletNode))
            return;

        if (inletNode.PlumbingNet == null)
            return;

        var neededTargets = new Dictionary<string, FixedPoint2>();
        var allMet = true;

        foreach (var (reagentProtoId, targetAmount) in ent.Comp.ReagentTargets)
        {
            var reagentId = reagentProtoId.Id;
            var currentAmount = buffer.GetReagentQuantity(new ReagentId(reagentId, null));
            var needed = targetAmount - currentAmount;
            if (needed > 0)
            {
                allMet = false;
                neededTargets[reagentId] = needed;
            }
        }

        if (neededTargets.Count > 0)
            _appearance.SetData(ent.Owner, PlumbingVisuals.Running, true);

        if (neededTargets.Count > 0)
            _pullSystem.PullSpecificReagents(ent.Owner, inletNode.PlumbingNet, bufferEnt.Value, neededTargets, ent.Comp.TransferAmount);

        if (allMet)
        {
            // Start heating only once reagents are ready
            if (_power.IsPowered(ent.Owner))
            {
                ApplyTemperatureControl(ent, bufferEnt.Value, buffer, args.dt);
            }

            // Dont do the reaction until we reach the the target temperature or close enough, but update the ui.
            if (!MathHelper.CloseTo(buffer.Temperature, ent.Comp.TargetTemperature, TemperatureTolerance))
            {
                UpdateUI(ent);
                return;
            }

            _reactionSystem.FullyReactSolution(bufferEnt.Value);

            var products = new List<(ReagentId Reagent, FixedPoint2 Quantity)>();
            foreach (var reagent in buffer.Contents)
            {
                // Skip target reagents still in the input buffer and only move products
                if (ent.Comp.ReagentTargets.ContainsKey(new ProtoId<ReagentPrototype>(reagent.Reagent.Prototype)))
                    continue;

                products.Add((reagent.Reagent, reagent.Quantity));
            }

            if (products.Count > 0)
            {
                foreach (var (reagent, quantity) in products)
                {
                    var removed = _solutionSystem.RemoveReagent(bufferEnt.Value, reagent, quantity);
                    if (removed > 0)
                        _solutionSystem.TryAddReagent(outputEnt.Value, reagent, removed, out _);
                }
            }

            if (products.Count > 0)
            {
                // Reset buffer to ambient temperature after any completed reaction pass.
                _solutionSystem.SetTemperature(bufferEnt.Value, Atmospherics.T20C);
                _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
            }
        }

        UpdateUI(ent);
    }

    /// <summary>
    ///     Applies temperature control by adding thermal energy to reach target temperature. Adds negative thermal energy for cooling.
    /// </summary>
    private void ApplyTemperatureControl(
        Entity<PlumbingReactorComponent> ent,
        Entity<SolutionComponent> solutionEnt,
        Solution solution,
        float dt)
    {
        var currentTemp = solution.Temperature;
        var targetTemp = ent.Comp.TargetTemperature;

        if (MathHelper.CloseTo(currentTemp, targetTemp, TemperatureTolerance))
            return;

        var heatCap = solution.GetHeatCapacity(_prototypeManager);
        if (heatCap <= 0f)
            return; // Don't heat empty solution

        // Calculate max energy we can transfer this tick in joules, watts * seconds = joules
        var maxEnergyTransfer = ent.Comp.HeatTransferPower * dt;

        var tempDiff = targetTemp - currentTemp;
        var energyNeeded = tempDiff * heatCap;

        var energyTransfer = Math.Clamp(energyNeeded, -maxEnergyTransfer, maxEnergyTransfer);

        _solutionSystem.AddThermalEnergy(solutionEnt, energyTransfer);
    }

    private void OnToggle(Entity<PlumbingReactorComponent> ent, ref PlumbingReactorToggleMessage args)
    {
        ent.Comp.Enabled = args.Enabled;
        DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.Enabled));
        ClickSound(ent.Owner);
        UpdateUI(ent);

        if (!args.Enabled)
            _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
    }

    private void OnSetTarget(Entity<PlumbingReactorComponent> ent, ref PlumbingReactorSetTargetMessage args)
    {
        if (args.Quantity <= 0)
        {
            if (_prototypeManager.HasIndex<ReagentPrototype>(args.ReagentId))
                ent.Comp.ReagentTargets.Remove(new ProtoId<ReagentPrototype>(args.ReagentId));
            DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.ReagentTargets));
            EvacuateBufferReagent(ent, args.ReagentId);
            UpdateUI(ent);
            return;
        }

        if (!_prototypeManager.HasIndex<ReagentPrototype>(args.ReagentId))
        {
            _popup.PopupEntity(Loc.GetString("plumbing-reactor-invalid-reagent", ("reagent", args.ReagentId)), ent.Owner, args.Actor);
            return;
        }

        // Clamp target quantity to remaining buffer capacity (max volume minus other targets)
        var clampedQuantity = args.Quantity;
        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var buffer))
        {
            var otherTargetsTotal = FixedPoint2.Zero;
            var key = new ProtoId<ReagentPrototype>(args.ReagentId);
            foreach (var (protoId, quantity) in ent.Comp.ReagentTargets)
            {
                if (protoId != key)
                    otherTargetsTotal += quantity;
            }

            var remaining = FixedPoint2.Max(buffer.MaxVolume - otherTargetsTotal, FixedPoint2.Zero);
            clampedQuantity = FixedPoint2.Min(clampedQuantity, remaining);
        }

        ent.Comp.ReagentTargets[new ProtoId<ReagentPrototype>(args.ReagentId)] = clampedQuantity;
        DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.ReagentTargets));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnRemoveTarget(Entity<PlumbingReactorComponent> ent, ref PlumbingReactorRemoveTargetMessage args)
    {
        ent.Comp.ReagentTargets.Remove(new ProtoId<ReagentPrototype>(args.ReagentId));
        DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.ReagentTargets));
        EvacuateBufferReagent(ent, args.ReagentId);
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnClearTargets(Entity<PlumbingReactorComponent> ent, ref PlumbingReactorClearTargetsMessage args)
    {
        ent.Comp.ReagentTargets.Clear();
        DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.ReagentTargets));
        EvacuateAllBufferReagents(ent);
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    /// <summary>
    ///     Moves a reagent out of the buffer into the output when its recipe target is removed,
    ///     so it stops showing in the list and doesn't clog the buffer. Only moves what the output
    ///     can hold; any remainder can still be plungered out.
    /// </summary>
    private void EvacuateBufferReagent(Entity<PlumbingReactorComponent> ent, string reagentId)
    {
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out var bufferEnt, out var buffer)
            || !_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.OutputSolutionName, out var outputEnt, out var output))
            return;

        var reagent = new ReagentId(reagentId, null);
        var toMove = FixedPoint2.Min(buffer.GetReagentQuantity(reagent), output.AvailableVolume);
        if (toMove <= 0)
            return;

        var removed = _solutionSystem.RemoveReagent(bufferEnt.Value, reagent, toMove);
        if (removed > 0)
            _solutionSystem.TryAddReagent(outputEnt.Value, reagent, removed, out _);
    }

    private void EvacuateAllBufferReagents(Entity<PlumbingReactorComponent> ent)
    {
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var buffer))
            return;

        // Snapshot ids first since evacuation mutates the buffer.
        var reagentIds = new List<string>();
        foreach (var reagent in buffer.Contents)
            reagentIds.Add(reagent.Reagent.Prototype);

        foreach (var reagentId in reagentIds)
            EvacuateBufferReagent(ent, reagentId);
    }

    private void OnSetTemperature(Entity<PlumbingReactorComponent> ent, ref PlumbingReactorSetTemperatureMessage args)
    {
        // Clamp to reasonable values
        ent.Comp.TargetTemperature = Math.Clamp(args.Temperature, MinTemperature, MaxTemperature);
        DirtyField(ent, ent.Comp, nameof(PlumbingReactorComponent.TargetTemperature));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnUIOpened(Entity<PlumbingReactorComponent> ent, ref BoundUIOpenedEvent args)
        => UpdateUI(ent);

    private void UpdateUI(Entity<PlumbingReactorComponent> ent)
    {
        var bufferContents = new Dictionary<string, FixedPoint2>();
        var outputContents = new Dictionary<string, FixedPoint2>();
        var currentTemperature = Atmospherics.T20C;

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var buffer))
        {
            foreach (var reagent in buffer.Contents)
            {
                bufferContents[reagent.Reagent.Prototype] = reagent.Quantity;
            }
            currentTemperature = buffer.Temperature;
        }

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.OutputSolutionName, out _, out var output))
        {
            foreach (var reagent in output.Contents)
            {
                outputContents[reagent.Reagent.Prototype] = reagent.Quantity;
            }
        }

        var reagentTargets = new Dictionary<string, FixedPoint2>();
        foreach (var (protoId, quantity) in ent.Comp.ReagentTargets)
        {
            reagentTargets[protoId.Id] = quantity;
        }

        var state = new PlumbingReactorBoundUserInterfaceState(
            reagentTargets,
            bufferContents,
            outputContents,
            ent.Comp.Enabled,
            ent.Comp.TargetTemperature,
            currentTemperature
        );

        _ui.SetUiState(ent.Owner, PlumbingReactorUiKey.Key, state);
    }

    private void ClickSound(EntityUid uid)
    {
        if (TryComp<PlumbingDeviceComponent>(uid, out var device))
            _audio.PlayPvs(device.ClickSound, uid, AudioParams.Default.WithVolume(-2f));
    }
}
