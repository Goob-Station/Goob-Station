using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Goobstation.Shared.LightDetection.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class DarknessAdaptionSystem : SharedDarknessAdaptionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DarknessAdaptionComponent, LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp, out var lightComp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            if (!comp.Active)
                continue;

            DoAbility((uid, comp), !lightComp.OnLight);
        }
    }
}
