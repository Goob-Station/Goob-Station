using Content.Goobstation.Shared.Supermatter.Components;
using Content.Server.GameTicking;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Supermatter.Systems;

public sealed class EndRoundAfterCascadeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EndRoundAfterCascadeComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, EndRoundAfterCascadeComponent component, MapInitEvent args)
    {
        _adminLogger.Add(LogType.Supermatter, LogImpact.High, $"SuperMatter cascade started, round will end in {component.Delay.TotalSeconds}s at {_timing.CurTime + component.Delay}");
    }

    public override void Update(float frameTime)
    {
        EndRoundAfterCascadeComponent? cascadeComp = null;

        var query = EntityQueryEnumerator<EndRoundAfterCascadeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Delay != TimeSpan.Zero)
            {
                cascadeComp = comp;
                break;
            }
        }

        if (cascadeComp == null)
            return;

        base.Update(frameTime);
        if ( cascadeComp.Delay < _gameTiming.CurTime && cascadeComp.Delay != TimeSpan.Zero)
        {
            cascadeComp.Delay = TimeSpan.Zero;
            _gameTicker.EndRound();
        }
    }
}
