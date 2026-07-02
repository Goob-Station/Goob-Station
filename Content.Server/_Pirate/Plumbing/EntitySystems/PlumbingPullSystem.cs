using Content.Server._Pirate.Plumbing.NodeGroups;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Provides methods for pulling reagents from plumbing networks.
///     All plumbing machines should use this system to pull from outlets on the network attached to their inlet.
///     Raises <see cref="PlumbingPullAttemptEvent"/> before each reagent pull, allowing other
///     systems (like filters) to deny specific reagents.
///     Machines try to pull from each outlet in their network using a "round-robin" style approach
///     to ensure fair distribution when multiple sources are available.
///     Supports outlets with valves (Enabled flag) and indirect solution lookup via container slots
///     (e.g., pulling from a beaker inside a dispenser).
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingPullSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    private EntityQuery<PlumbingOutletComponent> _outletQuery;

    public override void Initialize()
    {
        base.Initialize();
        _outletQuery = GetEntityQuery<PlumbingOutletComponent>();
    }

    /// <summary>
    ///     Pulls all allowed reagents from outlets on a plumbing network into a destination solution.
    /// </summary>
    /// <param name="puller">The entity doing the pulling.</param>
    /// <param name="network">The plumbing network to pull from.</param>
    /// <param name="destination">The solution to add pulled reagents to.</param>
    /// <param name="maxAmount">Maximum amount to pull total.</param>
    /// <param name="roundRobinIndex">
    ///     Index tracking which outlet to start from. The returned NextIndex should be
    ///     stored for the next call to cycle through outlets fairly. Initialize to 0.
    /// </param>
    /// <returns>Tuple of (amount pulled, next round-robin index to use).</returns>
    public (FixedPoint2 Pulled, int NextIndex) PullFromNetwork(
        EntityUid puller,
        IPlumbingNet network,
        Entity<SolutionComponent> destination,
        FixedPoint2 maxAmount,
        int roundRobinIndex)
    {
        var availableVolume = destination.Comp.Solution.AvailableVolume;
        var remaining = FixedPoint2.Min(maxAmount, availableVolume);
        if (remaining <= 0)
            return (FixedPoint2.Zero, roundRobinIndex);

        // Build list of valid outlets to pull from
        var outlets = new List<(PlumbingNode Node, PlumbingOutletComponent Outlet)>();
        foreach (var node in network.Nodes)
        {
            if (node is not PlumbingNode plumbingNode || plumbingNode.Owner == puller)
                continue;

            if (!_outletQuery.TryGetComponent(plumbingNode.Owner, out var outlet))
                continue;

            if (!outlet.Enabled)
                continue;

            var isOutletNode = false;
            foreach (var name in outlet.OutletNames)
            {
                if (plumbingNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    isOutletNode = true;
                    break;
                }
            }
            if (!isOutletNode)
                continue;

            outlets.Add((plumbingNode, outlet));
        }

        if (outlets.Count == 0)
            return (FixedPoint2.Zero, roundRobinIndex);

        // Round-robin: start from the saved index, wrap around
        // This ensures each outlet gets a fair chance to be pulled from first
        var startIndex = roundRobinIndex % outlets.Count;

        var totalPulled = FixedPoint2.Zero;

        for (var i = 0; i < outlets.Count && remaining > 0; i++)
        {
            var index = (startIndex + i) % outlets.Count;
            var (plumbingNode, outlet) = outlets[index];

            var pulled = PullFromOutlet(puller, plumbingNode.Owner, plumbingNode.Name, outlet, destination, remaining);
            totalPulled += pulled;
            remaining -= pulled;
        }

        var nextIndex = (startIndex + 1) % outlets.Count;
        return (totalPulled, nextIndex);
    }

    /// <summary>
    ///     Pulls reagents from outlets on a plumbing network into filtered and passthrough destinations.
    ///     Used by plumbing filters to split incoming reagents by configured filter list.
    /// </summary>
    public (FixedPoint2 Pulled, int NextIndex) PullFromNetworkSplit(
        EntityUid puller,
        IPlumbingNet network,
        Entity<SolutionComponent> filteredDestination,
        Entity<SolutionComponent> passthroughDestination,
        FixedPoint2 maxAmount,
        int roundRobinIndex,
        bool filterEnabled,
        HashSet<ProtoId<ReagentPrototype>> filteredReagents)
    {
        var remaining = maxAmount;
        if (remaining <= 0)
            return (FixedPoint2.Zero, roundRobinIndex);

        if (filteredDestination.Comp.Solution.AvailableVolume <= 0
            && passthroughDestination.Comp.Solution.AvailableVolume <= 0)
            return (FixedPoint2.Zero, roundRobinIndex);

        var outlets = new List<(PlumbingNode Node, PlumbingOutletComponent Outlet)>();
        foreach (var node in network.Nodes)
        {
            if (node is not PlumbingNode plumbingNode || plumbingNode.Owner == puller)
                continue;

            if (!_outletQuery.TryGetComponent(plumbingNode.Owner, out var outlet))
                continue;

            if (!outlet.Enabled)
                continue;

            var isOutletNode = false;
            foreach (var name in outlet.OutletNames)
            {
                if (plumbingNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    isOutletNode = true;
                    break;
                }
            }

            if (!isOutletNode)
                continue;

            outlets.Add((plumbingNode, outlet));
        }

        if (outlets.Count == 0)
            return (FixedPoint2.Zero, roundRobinIndex);

        var startIndex = roundRobinIndex % outlets.Count;
        var totalPulled = FixedPoint2.Zero;

        for (var i = 0; i < outlets.Count && remaining > 0; i++)
        {
            var index = (startIndex + i) % outlets.Count;
            var (plumbingNode, outlet) = outlets[index];

            var pulled = PullFromOutletSplit(
                puller,
                plumbingNode.Owner,
                plumbingNode.Name,
                outlet,
                filteredDestination,
                passthroughDestination,
                remaining,
                filterEnabled,
                filteredReagents);

            totalPulled += pulled;
            remaining -= pulled;
        }

        var nextIndex = (startIndex + 1) % outlets.Count;
        return (totalPulled, nextIndex);
    }

    /// <summary>
    ///     Pulls specific reagents from outlets on a plumbing network sequentially.
    ///     Used by the plumbing reactor to pull in its targeted reagents.
    /// </summary>
    /// <param name="puller">The entity doing the pulling.</param>
    /// <param name="network">The plumbing network to pull from.</param>
    /// <param name="destination">The solution to add pulled reagents to.</param>
    /// <param name="reagentTargets">Map of reagent ID to amount still needed.</param>
    /// <param name="transferLimit">Maximum total amount to transfer per update.</param>
    /// <returns>Map of reagent ID to amount actually pulled.</returns>
    public Dictionary<string, FixedPoint2> PullSpecificReagents(
        EntityUid puller,
        IPlumbingNet network,
        Entity<SolutionComponent> destination,
        Dictionary<string, FixedPoint2> reagentTargets,
        FixedPoint2 transferLimit)
    {
        var pulled = new Dictionary<string, FixedPoint2>();
        var destSolution = destination.Comp.Solution;
        var availableVolume = destSolution.AvailableVolume;
        var remaining = FixedPoint2.Min(transferLimit, availableVolume);

        if (remaining <= 0)
            return pulled;

        // Sequential pulling: fill each reagent fully before moving to the next
        foreach (var (reagentId, neededAmount) in reagentTargets)
        {
            if (remaining <= 0)
                break;

            if (neededAmount <= 0)
                continue;

            var stillNeeded = neededAmount;

            // Try to pull this reagent from all available outlets
            foreach (var node in network.Nodes)
            {
                if (remaining <= 0 || stillNeeded <= 0)
                    break;

                if (node is not PlumbingNode plumbingNode || plumbingNode.Owner == puller)
                    continue;

                if (!_outletQuery.TryGetComponent(plumbingNode.Owner, out var outlet))
                    continue;

                if (!outlet.Enabled)
                    continue;

                var isOutletNode = false;
                foreach (var name in outlet.OutletNames)
                {
                    if (plumbingNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        isOutletNode = true;
                        break;
                    }
                }
                if (!isOutletNode)
                    continue;

                if (GetOutletSolution(plumbingNode.Owner, plumbingNode.Name, outlet) is not { } sourceSoln)
                    continue;

                var available = sourceSoln.Comp.Solution.GetReagentQuantity(new ReagentId(reagentId, null));
                if (available <= 0)
                    continue;

                var attemptEv = new PlumbingPullAttemptEvent(puller, plumbingNode.Name, reagentId);
                RaiseLocalEvent(plumbingNode.Owner, ref attemptEv);

                if (attemptEv.Cancelled)
                    continue;

                var toPull = FixedPoint2.Min(available, stillNeeded);
                toPull = FixedPoint2.Min(toPull, remaining);

                var actualPulled = _solutionSystem.RemoveReagent(sourceSoln, new ReagentId(reagentId, null), toPull);
                if (actualPulled > 0)
                {
                    _solutionSystem.TryAddReagent(destination, new ReagentId(reagentId, null), actualPulled, out var actuallyAdded);

                    // Return any excess to source to prevent loss
                    var excess = actualPulled - actuallyAdded;
                    if (excess > 0)
                        _solutionSystem.TryAddReagent(sourceSoln, new ReagentId(reagentId, null), excess, out _);

                    pulled[reagentId] = pulled.GetValueOrDefault(reagentId, FixedPoint2.Zero) + actuallyAdded;
                    stillNeeded -= actuallyAdded;
                    remaining -= actuallyAdded;
                }
            }
        }

        return pulled;
    }

    /// <summary>
    ///     Pulls allowed reagents from a specific outlet into a destination solution.
    /// </summary>
    private FixedPoint2 PullFromOutlet(
        EntityUid puller,
        EntityUid sourceOwner,
        string nodeName,
        PlumbingOutletComponent outlet,
        Entity<SolutionComponent> destination,
        FixedPoint2 maxAmount)
    {
        if (GetOutletSolution(sourceOwner, nodeName, outlet) is not { } sourceSoln)
            return FixedPoint2.Zero;

        var sourceSolution = sourceSoln.Comp.Solution;
        if (sourceSolution.Volume <= 0)
            return FixedPoint2.Zero;

        var destSolution = destination.Comp.Solution;
        var availableSpace = destSolution.AvailableVolume;
        if (availableSpace <= 0)
            return FixedPoint2.Zero;

        var remaining = FixedPoint2.Min(maxAmount, availableSpace);

        // Build snapshot of allowed reagents to avoid modifying collection during iteration
        var allowedReagents = new List<(ReagentId Reagent, FixedPoint2 Quantity)>();

        foreach (var reagent in sourceSolution.Contents)
        {
            var attemptEv = new PlumbingPullAttemptEvent(puller, nodeName, reagent.Reagent.Prototype);
            RaiseLocalEvent(sourceOwner, ref attemptEv);

            if (!attemptEv.Cancelled)
                allowedReagents.Add((reagent.Reagent, reagent.Quantity));
        }

        if (allowedReagents.Count == 0)
            return FixedPoint2.Zero;

        var totalPulled = FixedPoint2.Zero;

        foreach (var (reagent, quantity) in allowedReagents)
        {
            if (remaining <= 0)
                break;

            var toPull = FixedPoint2.Min(quantity, remaining);
            if (toPull <= 0)
                continue;

            // Raise destination-side event — allows the puller to cap or deny the pull
            var intoEv = new PlumbingPullIntoAttemptEvent(sourceOwner, reagent.Prototype, toPull);
            RaiseLocalEvent(puller, ref intoEv);

            if (intoEv.Cancelled)
                continue;

            toPull = FixedPoint2.Min(toPull, intoEv.MaxAllowed);
            if (toPull <= 0)
                continue;

            var pulled = _solutionSystem.RemoveReagent(sourceSoln, reagent, toPull);
            if (pulled > 0)
            {
                _solutionSystem.TryAddReagent(destination, reagent, pulled, out var actuallyAdded);

                // Return any excess to source to prevent loss
                var excess = pulled - actuallyAdded;
                if (excess > 0)
                    _solutionSystem.TryAddReagent(sourceSoln, reagent, excess, out _);

                totalPulled += actuallyAdded;
                remaining -= actuallyAdded;
            }
        }

        return totalPulled;
    }

    /// <summary>
    ///     Pulls allowed reagents from a specific outlet into either filtered or passthrough destination.
    /// </summary>
    private FixedPoint2 PullFromOutletSplit(
        EntityUid puller,
        EntityUid sourceOwner,
        string nodeName,
        PlumbingOutletComponent outlet,
        Entity<SolutionComponent> filteredDestination,
        Entity<SolutionComponent> passthroughDestination,
        FixedPoint2 maxAmount,
        bool filterEnabled,
        HashSet<ProtoId<ReagentPrototype>> filteredReagents)
    {
        if (GetOutletSolution(sourceOwner, nodeName, outlet) is not { } sourceSoln)
            return FixedPoint2.Zero;

        var sourceSolution = sourceSoln.Comp.Solution;
        if (sourceSolution.Volume <= 0)
            return FixedPoint2.Zero;

        var remaining = maxAmount;

        var allowedReagents = new List<(ReagentId Reagent, FixedPoint2 Quantity)>();

        foreach (var reagent in sourceSolution.Contents)
        {
            var attemptEv = new PlumbingPullAttemptEvent(puller, nodeName, reagent.Reagent.Prototype);
            RaiseLocalEvent(sourceOwner, ref attemptEv);

            if (!attemptEv.Cancelled)
                allowedReagents.Add((reagent.Reagent, reagent.Quantity));
        }

        if (allowedReagents.Count == 0)
            return FixedPoint2.Zero;

        var totalPulled = FixedPoint2.Zero;

        foreach (var (reagent, quantity) in allowedReagents)
        {
            if (remaining <= 0)
                break;

            var isFiltered = filterEnabled && filteredReagents.Contains(new ProtoId<ReagentPrototype>(reagent.Prototype));
            var destination = isFiltered ? filteredDestination : passthroughDestination;

            var destinationAvailable = destination.Comp.Solution.AvailableVolume;
            if (destinationAvailable <= 0)
                continue;

            var toPull = FixedPoint2.Min(quantity, remaining);
            toPull = FixedPoint2.Min(toPull, destinationAvailable);
            if (toPull <= 0)
                continue;

            var intoEv = new PlumbingPullIntoAttemptEvent(sourceOwner, reagent.Prototype, toPull);
            RaiseLocalEvent(puller, ref intoEv);

            if (intoEv.Cancelled)
                continue;

            toPull = FixedPoint2.Min(toPull, intoEv.MaxAllowed);
            if (toPull <= 0)
                continue;

            var pulled = _solutionSystem.RemoveReagent(sourceSoln, reagent, toPull);
            if (pulled <= 0)
                continue;

            _solutionSystem.TryAddReagent(destination, reagent, pulled, out var actuallyAdded);

            var excess = pulled - actuallyAdded;
            if (excess > 0)
                _solutionSystem.TryAddReagent(sourceSoln, reagent, excess, out _);

            totalPulled += actuallyAdded;
            remaining -= actuallyAdded;
        }

        return totalPulled;
    }

    /// <summary>
    ///     Gets the solution entity to pull from for an outlet, handling pulling from beakers in container slots.
    ///     If ContainerSlotId is set, gets the solution from the entity in that item slot.
    ///     Otherwise, gets the solution directly from the outlet entity.
    /// </summary>
    /// <returns>The solution entity, or null if not found.</returns>
    private Entity<SolutionComponent>? GetOutletSolution(EntityUid outletOwner, string nodeName, PlumbingOutletComponent outlet)
    {
        EntityUid targetEntity;

        if (outlet.ContainerSlotId != null)
        {
            var containerEntity = _itemSlots.GetItemOrNull(outletOwner, outlet.ContainerSlotId);
            if (containerEntity == null)
                return null;
            targetEntity = containerEntity.Value;
        }
        else
        {
            targetEntity = outletOwner;
        }

        var solutionName = outlet.SolutionName;

        if (targetEntity == outletOwner
            && TryComp<PiratePlumbingFilterComponent>(outletOwner, out var filterComp))
        {
            if (nodeName.Equals(filterComp.FilterNodeName, StringComparison.OrdinalIgnoreCase))
                solutionName = filterComp.FilteredSolutionName;
            else if (nodeName.Equals(filterComp.PassthroughNodeName, StringComparison.OrdinalIgnoreCase))
                solutionName = filterComp.PassthroughSolutionName;
        }

        return _solutionSystem.TryGetSolution(targetEntity, solutionName, out var solutionEnt, out _)
            ? solutionEnt
            : null;
    }
}
