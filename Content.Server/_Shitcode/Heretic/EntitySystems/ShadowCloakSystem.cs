using Content.Server.IdentityManagement;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Server.Heretic.EntitySystems;

public sealed class ShadowCloakSystem : SharedShadowCloakSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    private static readonly TimeSpan SustainedDamageReductionInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdate = TimeSpan.Zero;

    protected override void Startup(Entity<ShadowCloakedComponent> ent)
    {
        base.Startup(ent);

        _identity.QueueIdentityUpdate(ent);
    }

    protected override void Shutdown(Entity<ShadowCloakedComponent> ent)
    {
        base.Shutdown(ent);

        _identity.QueueIdentityUpdate(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        if (_nextUpdate > now)
            return;

        _nextUpdate = now + SustainedDamageReductionInterval;

        var shadowCloakedQuery = EntityQueryEnumerator<ShadowCloakEntityComponent>();
        while (shadowCloakedQuery.MoveNext(out _, out var comp))
        {
            comp.SustainedDamage =
                FixedPoint2.Max(comp.SustainedDamage - comp.SustainedDamageReductionRate, FixedPoint2.Zero);
        }
    }
}
