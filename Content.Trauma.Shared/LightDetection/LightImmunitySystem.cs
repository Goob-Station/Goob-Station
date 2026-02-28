// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.LightDetection.Systems;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.LightDetection;

public sealed class LightImmunitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightImmunityComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<LightImmunityComponent, LightDamageUpdateAttemptEvent>(OnLightDamageUpdateAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var eqe = EntityQueryEnumerator<LightImmunityComponent>();
        while (eqe.MoveNext(out var uid, out var lightImmunity))
        {
            if (now < lightImmunity.NextUpdate)
                continue;

            RemCompDeferred<LightImmunityComponent>(uid);
        }
    }

    private void OnMapInit(Entity<LightImmunityComponent> ent, ref MapInitEvent args) =>
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Duration;

    private void OnLightDamageUpdateAttempt(Entity<LightImmunityComponent> ent, ref LightDamageUpdateAttemptEvent args) =>
        args.Cancelled = true;
}
