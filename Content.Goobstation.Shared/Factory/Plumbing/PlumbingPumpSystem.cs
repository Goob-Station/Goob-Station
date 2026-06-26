using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory.Plumbing;

public sealed class PlumbingPumpSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PlumbingFilterSystem _filter = default!;
    [Dependency] private readonly PlumbingLinkSystem _links = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    private EntityQuery<SolutionTransferComponent> _transferQuery;
    private readonly List<FilteredTransfer> _filteredTransfers = new();
    private readonly Dictionary<string, FilteredTransfer> _filteredTransferLookup = new();

    private sealed class FilteredTransfer
    {
        public string Reagent = string.Empty;
        public FixedPoint2 Amount;
        public FixedPoint2 Available;
        public FixedPoint2 Remaining;
    }

    public override void Initialize()
    {
        base.Initialize();
        _transferQuery = GetEntityQuery<SolutionTransferComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PlumbingPumpComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            comp.NextUpdate = now + comp.UpdateDelay;
            TryPump((uid, comp));
        }
    }

    private void TryPump(Entity<PlumbingPumpComponent> ent)
    {
        if (!_power.IsPowered(ent.Owner))
            return;

        if (!_links.TryGetInputSolution(ent, out var inputEnt)
            || !_links.TryResolveOutputChain(ent, out var processors, out var outputEnt)
            || !CanPump(ent.Owner, processors)
            || !TryBuildTransferPlan(processors, out var transferLimit, out var filteredTotal))
            return;

        var input = inputEnt.Comp.Solution;
        var output = outputEnt.Comp.Solution;

        var amount = FixedPoint2.Min(input.Volume, transferLimit);

        if (output.MaxVolume > FixedPoint2.Zero)
            amount = FixedPoint2.Min(amount, output.AvailableVolume);

        if (amount <= FixedPoint2.Zero)
            return;

        var split = SplitPlannedSolution(input, amount, filteredTotal);

        if (split.Volume <= FixedPoint2.Zero)
            return;

        _solution.UpdateChemicals(inputEnt, false); // removing reagents should never cause reactions? don't waste cpu updating it
        _solution.ForceAddSolution(outputEnt, split);
    }

    private bool CanPump(EntityUid owner, List<EntityUid> processors)
    {
        foreach (var processor in processors)
        {
            if (processor == owner)
                continue;

            if (!_power.IsPowered(processor))
                return false;
        }

        return true;
    }

    private bool TryBuildTransferPlan(List<EntityUid> processors, out FixedPoint2 transferLimit, out FixedPoint2 filteredTotal)
    {
        _filteredTransfers.Clear();

        transferLimit = FixedPoint2.Zero;
        filteredTotal = FixedPoint2.Zero;

        var maximumLimit = FixedPoint2.MaxValue;
        var unfilteredLimit = FixedPoint2.MaxValue;
        var hasUnfilteredProcessor = false;

        foreach (var processor in processors)
        {
            if (!_transferQuery.TryComp(processor, out var transfer))
                return false;

            maximumLimit = FixedPoint2.Min(maximumLimit, transfer.MaximumTransferAmount);

            if (_filter.GetFilteredReagent(processor) is { } reagent)
            {
                AddFilteredTransfer(reagent, transfer.TransferAmount);
                filteredTotal += transfer.TransferAmount;
                continue;
            }

            hasUnfilteredProcessor = true;
            unfilteredLimit = FixedPoint2.Min(unfilteredLimit, transfer.TransferAmount);
        }

        if (_filteredTransfers.Count == 0)
        {
            transferLimit = hasUnfilteredProcessor
                ? FixedPoint2.Min(unfilteredLimit, maximumLimit)
                : FixedPoint2.Zero;

            return transferLimit > FixedPoint2.Zero;
        }

        transferLimit = filteredTotal;

        if (hasUnfilteredProcessor)
            transferLimit = FixedPoint2.Min(transferLimit, unfilteredLimit);

        transferLimit = FixedPoint2.Min(transferLimit, maximumLimit);
        return transferLimit > FixedPoint2.Zero;
    }

    private void AddFilteredTransfer(string reagent, FixedPoint2 amount)
    {
        foreach (var transfer in _filteredTransfers)
        {
            if (transfer.Reagent != reagent)
                continue;

            transfer.Amount += amount;
            return;
        }

        _filteredTransfers.Add(new FilteredTransfer
        {
            Reagent = reagent,
            Amount = amount,
        });
    }

    private Solution SplitPlannedSolution(Solution input, FixedPoint2 limit, FixedPoint2 filteredTotal)
    {
        if (_filteredTransfers.Count == 0)
            return input.SplitSolution(FixedPoint2.Min(input.Volume, limit));

        if (filteredTotal <= FixedPoint2.Zero)
            return new Solution();

        return SplitFilteredSolution(input, limit);
    }

    private Solution SplitFilteredSolution(Solution input, FixedPoint2 limit)
    {
        _filteredTransferLookup.Clear();

        foreach (var transfer in _filteredTransfers)
        {
            transfer.Available = FixedPoint2.Zero;
            transfer.Remaining = FixedPoint2.Zero;
            _filteredTransferLookup[transfer.Reagent] = transfer;
        }

        foreach (var (reagent, quantity) in input.Contents)
        {
            if (_filteredTransferLookup.TryGetValue(reagent.Prototype, out var transfer))
                transfer.Available += quantity;
        }

        var effectiveTotal = FixedPoint2.Zero;

        foreach (var transfer in _filteredTransfers)
        {
            if (transfer.Available <= FixedPoint2.Zero)
                continue;

            transfer.Remaining = FixedPoint2.Min(transfer.Amount, transfer.Available);
            effectiveTotal += transfer.Remaining;
        }

        if (effectiveTotal <= FixedPoint2.Zero)
            return new Solution();

        if (effectiveTotal > limit)
        {
            var remainingLimit = limit.Value;
            var remainingTotal = effectiveTotal.Value;

            foreach (var transfer in _filteredTransfers)
            {
                if (transfer.Remaining <= FixedPoint2.Zero)
                    continue;

                var requested = transfer.Remaining.Value;
                var scaled = (long) requested * remainingLimit / remainingTotal;

                if (scaled <= 0 && remainingLimit > 0)
                    scaled = 1;

                if (scaled > remainingLimit)
                    scaled = remainingLimit;

                transfer.Remaining = FixedPoint2.FromCents((int) scaled);

                remainingLimit -= transfer.Remaining.Value;
                remainingTotal -= requested;
            }
        }

        var requestedCount = 0;

        foreach (var transfer in _filteredTransfers)
        {
            if (transfer.Remaining > FixedPoint2.Zero)
                requestedCount++;
        }

        if (requestedCount == 0)
            return new Solution();

        var remainingContents = new List<ReagentQuantity>(input.Contents.Count);
        var splitContents = new List<ReagentQuantity>(requestedCount);

        foreach (var (reagent, quantity) in input.Contents)
        {
            if (!_filteredTransferLookup.TryGetValue(reagent.Prototype, out var transfer)
                || transfer.Remaining <= FixedPoint2.Zero)
            {
                remainingContents.Add(new ReagentQuantity(reagent, quantity));
                continue;
            }

            var available = transfer.Available;
            var splitQuantity = quantity >= available
                ? FixedPoint2.Min(transfer.Remaining, quantity)
                : FixedPoint2.FromCents((int) ((long) transfer.Remaining.Value * quantity.Value / available.Value));

            transfer.Available = available - quantity;
            transfer.Remaining -= splitQuantity;

            if (splitQuantity > FixedPoint2.Zero)
                splitContents.Add(new ReagentQuantity(reagent, splitQuantity));

            var left = quantity - splitQuantity;

            if (left > FixedPoint2.Zero)
                remainingContents.Add(new ReagentQuantity(reagent, left));
        }

        var split = new Solution(splitContents, false)
        {
            Temperature = input.Temperature,
        };

        input.SetContents(remainingContents);
        return split;
    }
}
