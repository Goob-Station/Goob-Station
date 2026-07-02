using System.Numerics;
using Content.Server._Pirate.Plumbing.Components;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.NodeContainer;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using SharedAppearanceSystem = Robust.Shared.GameObjects.SharedAppearanceSystem;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles the plumbing pill press: pulls reagents from the inlet network into a buffer,
///     and automatically creates pills or patches when the buffer has enough for the set dosage.
///     Supports optional mixing mode with two ratio-controlled inlets (E/W).
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingPillPressSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private PlumbingPullSystem _pullSystem = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private LabelSystem _labelSystem = default!;
    [Dependency] private TagSystem _tag = default!;

    private static readonly EntProtoId _pillPrototypeId = "Pill";
    // Pirate: local fork uses Goob medical patches instead of Starlight's Patch entity.
    private static readonly EntProtoId _patchPrototypeId = "MedicalPatchBasic";
    private static readonly ProtoId<TagPrototype> _medicalPatchTag = "MedicalPatch";
    private const int MaxOutputEntitiesOnTile = 30;

    /// <summary>Max dosage matches the ChemMaster limit.</summary>
    private const uint MaxDosage = 20;
    private const uint MinDosage = 1;
    private const uint MaxPillTypes = SharedChemMaster.PillTypes;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingDeviceUpdateEvent>(OnDeviceUpdate);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressToggleMessage>(OnToggle);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetDosageMessage>(OnSetDosage);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetLabelMessage>(OnSetLabel);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetOutputModeMessage>(OnSetOutputMode);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetPillTypeMessage>(OnSetPillType);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetMixingMessage>(OnSetMixing);
        SubscribeLocalEvent<PlumbingPillPressComponent, PlumbingPillPressSetInletRatioMessage>(OnSetInletRatio);
        SubscribeLocalEvent<PlumbingPillPressComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnDeviceUpdate(Entity<PlumbingPillPressComponent> ent, ref PlumbingDeviceUpdateEvent args)
    {
        if (!ent.Comp.Enabled)
        {
            _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
            return;
        }

        if (ent.Comp.MixingEnabled)
            HandleMixingPull(ent);

        // Pulling from the normal N inlet is handled by PlumbingInletSystem via PlumbingInletComponent.
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out var solutionEnt, out var solution))
            return;

        var dosage = FixedPoint2.New(ent.Comp.Dosage);
        var produced = false;

        if (solution.Volume >= dosage)
        {
            // Spawn on the same tile, offset slightly south
            var spawnCoords = Transform(ent.Owner).Coordinates.Offset(new Vector2(0, -0.3f));

            if (GetOutputEntityCount(spawnCoords) >= MaxOutputEntitiesOnTile)
            {
                _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
                UpdateUiState(ent);
                return;
            }

            var withdrawal = _solutionSystem.SplitSolution(solutionEnt.Value, dosage);

            if (withdrawal.Volume <= FixedPoint2.Zero)
            {
                _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
                UpdateUiState(ent);
                return;
            }

            produced = true;

            if (ent.Comp.OutputMode == PillPressOutputMode.Pill)
            {
                var item = Spawn(_pillPrototypeId, spawnCoords);
                _labelSystem.Label(item, ent.Comp.Label);
                _solutionSystem.EnsureSolutionEntity(item,
                    SharedChemMaster.PillSolutionName,
                    out var itemSolution,
                    dosage);

                if (itemSolution.HasValue)
                    _solutionSystem.TryAddSolution(itemSolution.Value, withdrawal);

                var pill = Comp<PillComponent>(item);
                pill.PillType = ent.Comp.PillType;
                Dirty(item, pill);
            }
            else
            {
                var item = Spawn(_patchPrototypeId, spawnCoords);
                _labelSystem.Label(item, ent.Comp.Label);

                _solutionSystem.EnsureSolutionEntity(item,
                    SharedChemMaster.BottleSolutionName,
                    out var itemSolution,
                    dosage);

                if (itemSolution.HasValue)
                    _solutionSystem.TryAddSolution(itemSolution.Value, withdrawal);
            }
        }

        _appearance.SetData(ent.Owner, PlumbingVisuals.Running, produced || solution.Volume >= dosage);

        UpdateUiState(ent);
    }

    /// <summary>
    ///     Handles the mixing mode: pulls from E/W inlets into staging solutions at set ratios,
    ///     then combines into the main buffer when both targets are met.
    /// </summary>
    private void HandleMixingPull(Entity<PlumbingPillPressComponent> ent)
    {
        var totalRatio = ent.Comp.InletRatioEast + ent.Comp.InletRatioWest;
        if (totalRatio <= 0)
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out var bufferEnt, out var buffer))
            return;

        // Don't pull more if buffer already has enough for a pill
        var dosage = FixedPoint2.New(ent.Comp.Dosage);
        if (buffer.Volume >= dosage)
            return;

        var eastFraction = ent.Comp.InletRatioEast / totalRatio;
        var eastTarget = FixedPoint2.New((int) MathF.Round(eastFraction * (float) dosage));
        var westTarget = dosage - eastTarget; // Remainder goes to west to avoid rounding loss

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.StagingEastSolutionName, out var stagingEastEnt, out var stagingEast))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.StagingWestSolutionName, out var stagingWestEnt, out var stagingWest))
            return;

        if (!TryComp<NodeContainerComponent>(ent.Owner, out var nodeContainer))
            return;

        if (ent.Comp.InletRatioEast > 0 && stagingEast.Volume < eastTarget)
        {
            if (nodeContainer.Nodes.TryGetValue(ent.Comp.InletEastNodeName, out var eastNode)
                && eastNode is PlumbingNode eastPlumbingNode
                && eastPlumbingNode.PlumbingNet != null)
            {
                var eastNeeded = eastTarget - stagingEast.Volume;
                _pullSystem.PullFromNetwork(ent.Owner, eastPlumbingNode.PlumbingNet, stagingEastEnt.Value, eastNeeded, 0);
            }
        }

        if (ent.Comp.InletRatioWest > 0 && stagingWest.Volume < westTarget)
        {
            if (nodeContainer.Nodes.TryGetValue(ent.Comp.InletWestNodeName, out var westNode)
                && westNode is PlumbingNode westPlumbingNode
                && westPlumbingNode.PlumbingNet != null)
            {
                var westNeeded = westTarget - stagingWest.Volume;
                _pullSystem.PullFromNetwork(ent.Owner, westPlumbingNode.PlumbingNet, stagingWestEnt.Value, westNeeded, 0);
            }
        }

        var eastMet = ent.Comp.InletRatioEast <= 0 || stagingEast.Volume >= eastTarget;
        var westMet = ent.Comp.InletRatioWest <= 0 || stagingWest.Volume >= westTarget;

        if (!eastMet || !westMet)
            return;

        if (stagingEast.Volume > 0)
        {
            var eastWithdrawal = _solutionSystem.SplitSolution(stagingEastEnt.Value, stagingEast.Volume);
            _solutionSystem.TryAddSolution(bufferEnt.Value, eastWithdrawal);
        }

        if (stagingWest.Volume > 0)
        {
            var westWithdrawal = _solutionSystem.SplitSolution(stagingWestEnt.Value, stagingWest.Volume);
            _solutionSystem.TryAddSolution(bufferEnt.Value, westWithdrawal);
        }
    }

    private void OnToggle(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressToggleMessage args)
    {
        ent.Comp.Enabled = args.Enabled;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.Enabled));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetDosage(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetDosageMessage args)
    {
        var dosage = Math.Clamp(args.Dosage, MinDosage, MaxDosage);
        ent.Comp.Dosage = dosage;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.Dosage));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetLabel(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetLabelMessage args)
    {
        if (args.Label.Length > SharedChemMaster.LabelMaxLength)
            return;

        ent.Comp.Label = args.Label;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.Label));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetOutputMode(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetOutputModeMessage args)
    {
        ent.Comp.OutputMode = args.OutputMode;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.OutputMode));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetPillType(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetPillTypeMessage args)
    {
        if (args.PillType >= MaxPillTypes)
            return;

        ent.Comp.PillType = args.PillType;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.PillType));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetMixing(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetMixingMessage args)
    {
        ent.Comp.MixingEnabled = args.MixingEnabled;
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.MixingEnabled));
        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnSetInletRatio(Entity<PlumbingPillPressComponent> ent, ref PlumbingPillPressSetInletRatioMessage args)
    {
        var ratio = Math.Clamp(args.Ratio, 0f, 100f);
        var complement = 100f - ratio;

        switch (args.Inlet)
        {
            case PillPressInlet.East:
                ent.Comp.InletRatioEast = ratio;
                ent.Comp.InletRatioWest = complement;
                break;
            case PillPressInlet.West:
                ent.Comp.InletRatioWest = ratio;
                ent.Comp.InletRatioEast = complement;
                break;
            default:
                return;
        }

        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.InletRatioEast));
        DirtyField(ent, ent.Comp, nameof(PlumbingPillPressComponent.InletRatioWest));

        ClickSound(ent);
        UpdateUiState(ent);
    }

    private void OnUIOpened(Entity<PlumbingPillPressComponent> ent, ref BoundUIOpenedEvent args)
        => UpdateUiState(ent);

    private void UpdateUiState(Entity<PlumbingPillPressComponent> ent)
    {
        var bufferVolume = FixedPoint2.Zero;

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var solution))
        {
            bufferVolume = solution.Volume;
        }

        var stagingEastVolume = FixedPoint2.Zero;
        var stagingWestVolume = FixedPoint2.Zero;

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.StagingEastSolutionName, out _, out var stagingEast))
            stagingEastVolume = stagingEast.Volume;

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.StagingWestSolutionName, out _, out var stagingWest))
            stagingWestVolume = stagingWest.Volume;

        var state = new PlumbingPillPressBoundUserInterfaceState(
            bufferVolume,
            ent.Comp.Dosage,
            ent.Comp.OutputMode,
            ent.Comp.PillType,
            ent.Comp.Label,
            ent.Comp.Enabled,
            ent.Comp.MixingEnabled,
            ent.Comp.InletRatioEast,
            ent.Comp.InletRatioWest,
            stagingEastVolume,
            stagingWestVolume);

        _ui.SetUiState(ent.Owner, PlumbingPillPressUiKey.Key, state);
    }

    private void ClickSound(EntityUid uid)
    {
        if (TryComp<PlumbingDeviceComponent>(uid, out var device))
            _audio.PlayPvs(device.ClickSound, uid, AudioParams.Default.WithVolume(-2f));
    }

    private int GetOutputEntityCount(EntityCoordinates coords)
    {
        var count = 0;

        foreach (var entity in _lookup.GetEntitiesIntersecting(coords))
        {
            if (HasComp<PillComponent>(entity) || _tag.HasTag(entity, _medicalPatchTag))
                count++;
        }

        return count;
    }
}
