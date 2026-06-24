using Content.Server._Pirate.Plumbing.Components;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Server.Popups;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.NodeContainer;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles plumbing filter behavior and filter control UI.
///     Intake routing into filtered/passthrough lanes is handled here on device update.
///     Outlets still enforce reagent restrictions by node:
///     - Filter outlet: only allows pulling reagents matching the filter list
///     - Passthrough outlet: only allows pulling reagents NOT matching the filter list
///     Restriction is enforced via PlumbingPullAttemptEvent.
/// </summary>
[UsedImplicitly]
public sealed partial class PiratePlumbingFilterSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private PlumbingPullSystem _pullSystem = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PiratePlumbingFilterComponent, PlumbingPullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, PlumbingDeviceUpdateEvent>(OnDeviceUpdate);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, PiratePlumbingFilterToggleMessage>(OnToggle);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, PiratePlumbingFilterAddReagentMessage>(OnAddReagent);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, PiratePlumbingFilterRemoveReagentMessage>(OnRemoveReagent);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, PiratePlumbingFilterClearMessage>(OnClear);
        SubscribeLocalEvent<PiratePlumbingFilterComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    /// <summary>
    ///     Handles pull attempts - restricts which reagents can be pulled based on outlet node.
    /// </summary>
    private void OnPullAttempt(Entity<PiratePlumbingFilterComponent> ent, ref PlumbingPullAttemptEvent args)
    {
        // When disabled, block the filter outlet entirely — everything goes through passthrough
        if (!ent.Comp.Enabled)
        {
            if (args.NodeName == ent.Comp.FilterNodeName)
                args.Cancelled = true;
            return;
        }

        var isFilteredReagent = ent.Comp.FilteredReagents.Contains(args.ReagentPrototype);

        if (args.NodeName == ent.Comp.FilterNodeName)
        {
            if (!isFilteredReagent)
                args.Cancelled = true;
        }
        else if (args.NodeName == ent.Comp.PassthroughNodeName)
        {
            if (isFilteredReagent)
                args.Cancelled = true;
        }
    }

    private void OnDeviceUpdate(Entity<PiratePlumbingFilterComponent> ent, ref PlumbingDeviceUpdateEvent args)
    {
        if (!TryComp<PlumbingInletComponent>(ent.Owner, out var inlet))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.FilteredSolutionName, out var filteredEnt, out var filteredSolution))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.PassthroughSolutionName, out var passthroughEnt, out var passthroughSolution))
            return;

        if (filteredSolution.AvailableVolume <= 0 && passthroughSolution.AvailableVolume <= 0)
            return;

        if (!TryComp<NodeContainerComponent>(ent.Owner, out var nodeContainer))
            return;

        var remaining = inlet.TransferAmount;

        foreach (var inletName in inlet.InletNames)
        {
            if (remaining <= 0)
                break;

            if (filteredSolution.AvailableVolume <= 0 && passthroughSolution.AvailableVolume <= 0)
                break;

            if (!nodeContainer.Nodes.TryGetValue(inletName, out var node))
                continue;

            if (node is not PlumbingNode plumbingNode || plumbingNode.PlumbingNet == null)
                continue;

            var roundRobinIndex = inlet.RoundRobinIndices.GetValueOrDefault(inletName, 0);
            var (pulled, nextIndex) = _pullSystem.PullFromNetworkSplit(
                ent.Owner,
                plumbingNode.PlumbingNet,
                filteredEnt.Value,
                passthroughEnt.Value,
                remaining,
                roundRobinIndex,
                ent.Comp.Enabled,
                ent.Comp.FilteredReagents);

            inlet.RoundRobinIndices[inletName] = nextIndex;
            remaining -= pulled;
        }
    }

    private void OnToggle(Entity<PiratePlumbingFilterComponent> ent, ref PiratePlumbingFilterToggleMessage args)
    {
        ent.Comp.Enabled = args.Enabled;
        DirtyField(ent, ent.Comp, nameof(PiratePlumbingFilterComponent.Enabled));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnAddReagent(Entity<PiratePlumbingFilterComponent> ent, ref PiratePlumbingFilterAddReagentMessage args)
    {
        if (!_prototypeManager.HasIndex<ReagentPrototype>(args.ReagentId))
        {
            _popup.PopupEntity(Loc.GetString("pirate-plumbing-filter-invalid-reagent", ("reagent", args.ReagentId)), ent.Owner, args.Actor);
            return;
        }

        var reagentProtoId = new ProtoId<ReagentPrototype>(args.ReagentId);

        if (!ent.Comp.FilteredReagents.Contains(reagentProtoId)
            && ent.Comp.FilteredReagents.Count >= PiratePlumbingFilterComponent.MaxFilteredReagents)
        {
            _popup.PopupEntity(
                Loc.GetString("pirate-plumbing-filter-max-reagents", ("count", PiratePlumbingFilterComponent.MaxFilteredReagents)),
                ent.Owner,
                args.Actor);
            return;
        }

        ent.Comp.FilteredReagents.Add(reagentProtoId);
        DirtyField(ent, ent.Comp, nameof(PiratePlumbingFilterComponent.FilteredReagents));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnRemoveReagent(Entity<PiratePlumbingFilterComponent> ent, ref PiratePlumbingFilterRemoveReagentMessage args)
    {
        ent.Comp.FilteredReagents.Remove(new ProtoId<ReagentPrototype>(args.ReagentId));
        DirtyField(ent, ent.Comp, nameof(PiratePlumbingFilterComponent.FilteredReagents));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnClear(Entity<PiratePlumbingFilterComponent> ent, ref PiratePlumbingFilterClearMessage args)
    {
        ent.Comp.FilteredReagents.Clear();
        DirtyField(ent, ent.Comp, nameof(PiratePlumbingFilterComponent.FilteredReagents));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnUIOpened(Entity<PiratePlumbingFilterComponent> ent, ref BoundUIOpenedEvent args)
        => UpdateUI(ent);

    private void UpdateUI(Entity<PiratePlumbingFilterComponent> ent)
    {
        // Convert ProtoId to string for UI state
        var filteredReagents = new HashSet<string>();
        foreach (var protoId in ent.Comp.FilteredReagents)
        {
            filteredReagents.Add(protoId.Id);
        }

        var state = new PiratePlumbingFilterBoundUserInterfaceState(
            filteredReagents,
            ent.Comp.Enabled);

        _ui.SetUiState(ent.Owner, PiratePlumbingFilterUiKey.Key, state);
    }

    private void ClickSound(EntityUid uid)
    {
        if (TryComp<PlumbingDeviceComponent>(uid, out var device))
            _audio.PlayPvs(device.ClickSound, uid, AudioParams.Default.WithVolume(-2f));
    }
}
