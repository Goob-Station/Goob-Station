using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory.Plumbing;

public sealed class PlumbingPumpSystem : EntitySystem
{
    [Dependency] private readonly ExclusiveSlotsSystem _exclusive = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PlumbingFilterSystem _filter = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    private EntityQuery<SolutionTransferComponent> _transferQuery;

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

        // pump does nothing unless both slots are linked
        if (_exclusive.GetInputSlot(ent)?.GetSolution() is not {} inputEnt
            || _exclusive.GetOutputSlot(ent)?.GetSolution() is not {} outputEnt)
            return;

        var input = inputEnt.Comp.Solution;
        var output = outputEnt.Comp.Solution;
        var limit = _transferQuery.Comp(ent).TransferAmount;

        var amount = FixedPoint2.Min(input.Volume, limit);
        if (output.MaxVolume > FixedPoint2.Zero)
            amount = FixedPoint2.Min(amount, output.AvailableVolume);
        if (amount <= FixedPoint2.Zero)
            return;

        var split = _filter.GetFilteredReagent(ent) is {} filter
            ? input.SplitSolutionWithOnly(amount, filter)
            : input.SplitSolution(amount);
        _solution.UpdateChemicals(inputEnt, false); // removing reagents should never cause reactions? don't waste cpu updating it
        _solution.ForceAddSolution(outputEnt, split);
    }
}
