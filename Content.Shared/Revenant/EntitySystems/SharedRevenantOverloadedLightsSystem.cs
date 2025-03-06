using Content.Shared.Beam;
using Content.Shared.Revenant.Components;

namespace Content.Shared.Revenant.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedRevenantOverloadedLightsSystem : EntitySystem
{
    [Dependency] private readonly SharedBeamSystem _beam = default!; // Goobstation

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<RevenantOverloadedLightsComponent>();

        while (enumerator.MoveNext(out var uid, out var comp))
        {
            comp.Accumulator += frameTime;

            if (comp.Accumulator < comp.ZapDelay)
                continue;

            OnZap((uid, comp));
            RemCompDeferred(uid, comp);
        }

        _beam.AccumulateIndex(); // Goobstation
    }

    protected abstract void OnZap(Entity<RevenantOverloadedLightsComponent> component);
}
