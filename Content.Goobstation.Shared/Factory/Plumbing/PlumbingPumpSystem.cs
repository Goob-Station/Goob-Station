using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory.Plumbing;

public sealed class PlumbingPumpSystem : EntitySystem
{
    [Dependency] private readonly ExclusiveSlotsSystem _exclusive = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PlumbingFilterSystem _filter = default!;
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
        // pump does nothing unless both slots are linked
        if (_exclusive.GetInputSlot(ent) is not AutomatedSolution inputSlot ||
            _exclusive.GetOutputSlot(ent) is not AutomatedSolution outputSlot)
            return;

        var input = inputSlot.Solution.Comp.Solution;
        var output = outputSlot.Solution;

        var limit = _transferQuery.Comp(ent).TransferAmount;
        if (_filter.GetFilteredReagent(ent) is not {} filter)
        {
            _solution.TryTransferSolution(output, input, limit);
            return;
        }

        // basically a TryTransferSolutionWithOnly
        var amount = FixedPoint2.Min(input.Volume, output.Comp.Solution.AvailableVolume, limit);
        var split = input.SplitSolutionWithOnly(amount, filter);
        _solution.AddSolution(output, split);
    }
}
