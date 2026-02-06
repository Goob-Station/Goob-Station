using Content.Server.IdentityManagement;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server.Heretic.EntitySystems;

public sealed class ShadowCloakSystem : SharedShadowCloakSystem
{
    [Dependency] private readonly IdentitySystem _identity = default!;

    private const float SustainedDamageReductionInterval = 1f;
    private float _accumulator;

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

        var shadowEntityQuery = AllEntityQuery<ShadowCloakEntityComponent>();
        while (shadowEntityQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.DeletionAccumulator == null)
                continue;

            comp.DeletionAccumulator -= frameTime;

            if (comp.DeletionAccumulator > 0)
                continue;

            QueueDel(uid);
        }

        _accumulator += frameTime;

        if (_accumulator < SustainedDamageReductionInterval)
            return;

        _accumulator = 0f;

        var shadowCloakedQuery = EntityQueryEnumerator<ShadowCloakedComponent>();
        while (shadowCloakedQuery.MoveNext(out _, out var comp))
        {
            comp.SustainedDamage =
                FixedPoint2.Max(comp.SustainedDamage - comp.SustainedDamageReductionRate, FixedPoint2.Zero);
        }
    }
}
