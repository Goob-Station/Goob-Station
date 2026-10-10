using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Prototypes;
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
    private readonly Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> _filteredTransferAmounts = new();

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
        if (!_links.TryGetInputSolution(ent.Owner, out var inputEnt)
            || !_links.TryResolveOutputChain(ent.Owner, out var pumps, out var outputEnt)
            || !TryBuildTransferPlan(pumps, out var transferLimit))
            return;

        var input = inputEnt.Comp.Solution;
        var output = outputEnt.Comp.Solution;
        var outputLimit = transferLimit;

        if (output.MaxVolume > FixedPoint2.Zero)
            outputLimit = FixedPoint2.Min(outputLimit, output.AvailableVolume);

        if (outputLimit <= FixedPoint2.Zero)
            return;

        var split = _filteredTransferAmounts.Count == 0
            ? input.SplitSolution(outputLimit)
            : SplitFilteredSolution(input, transferLimit, outputLimit);

        if (split.Volume <= FixedPoint2.Zero)
            return;

        _solution.UpdateChemicals(inputEnt, false); // removing reagents should never cause reactions? don't waste cpu updating it
        _solution.ForceAddSolution(outputEnt, split);
    }

    private bool TryBuildTransferPlan(List<EntityUid> pumps, out FixedPoint2 transferLimit)
    {
        _filteredTransferAmounts.Clear();
        transferLimit = FixedPoint2.Zero;

        foreach (var pump in pumps)
        {
            if (!_power.IsPowered(pump)
                || !_transferQuery.TryComp(pump, out var transfer))
            {
                return false;
            }

            if (transfer.TransferAmount <= FixedPoint2.Zero)
            {
                continue;
            }

            transferLimit += transfer.TransferAmount;

            if (_filter.GetFilteredReagent(pump) is { } reagent)
            {
                if (!_filteredTransferAmounts.TryAdd(reagent, transfer.TransferAmount))
                {
                    _filteredTransferAmounts[reagent] += transfer.TransferAmount;
                }
            }
        }

        return transferLimit > FixedPoint2.Zero;
    }

    private Solution SplitFilteredSolution(Solution input, FixedPoint2 transferLimit, FixedPoint2 outputLimit)
    {
        var filteredTransferAmount = _filteredTransferAmounts.Values.Sum();
        var transferScale = transferLimit.Double() / filteredTransferAmount.Double();
        var requestedTotal = FixedPoint2.Zero;

        foreach (var (reagent, transferAmount) in _filteredTransferAmounts)
        {
            var available = input.GetTotalPrototypeQuantity(reagent.Id);
            requestedTotal += FixedPoint2.Min(available, transferAmount * transferScale);
        }

        if (requestedTotal <= FixedPoint2.Zero)
        {
            return new Solution();
        }

        var outputScale = Math.Min(1.0, outputLimit.Double() / requestedTotal.Double());
        var filtered = new Solution
        {
            Temperature = input.Temperature,
        };

        foreach (var (reagent, transferAmount) in _filteredTransferAmounts)
        {
            var available = input.GetTotalPrototypeQuantity(reagent.Id);
            var reagentAmount = FixedPoint2.Min(available, transferAmount * transferScale) * outputScale;

            if (reagentAmount <= FixedPoint2.Zero)
            {
                continue;
            }

            foreach (var quantity in input.SplitSolutionWithOnly(reagentAmount, reagent.Id))
            {
                filtered.AddReagent(quantity);
            }
        }

        return filtered;
    }
}
