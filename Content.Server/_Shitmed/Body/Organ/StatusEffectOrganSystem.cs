using Content.Shared.Body.Organ;
using Content.Shared.StatusEffect;
using Robust.Shared.Timing;

namespace Content.Server._Shitmed.Body.Organ;

public sealed class StatusEffectOrganSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _effects = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StatusEffectOrganComponent, OrganComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var organ))
        {
            if (now < comp.NextUpdate || organ.Body is not {} body)
                continue;

            comp.NextUpdate = now + comp.Delay;
            if (!TryComp<StatusEffectsComponent>(body, out var effects))
                continue;

            foreach (var (key, component) in comp.Refresh)
            {
                _effects.TryAddStatusEffect(body, key, comp.Delay, refresh: true, component, effects);
            }
        }
    }
}
