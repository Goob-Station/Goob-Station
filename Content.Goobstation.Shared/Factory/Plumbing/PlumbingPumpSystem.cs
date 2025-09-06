using Content.Shared.Chemistry.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory.Plumbing;

public sealed class PlumbingPumpSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

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
        // TODO
        var amount = _transferQuery.Comp(ent).TransferAmount;
        
    }
}
