using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

/// <summary>
/// This handles throwing the hooked entity towards the user
/// </summary>
public sealed class TentacleHookedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TentacleHookedComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<TentacleHookedComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            if (!CheckDistance((uid, comp)))
                continue;

            Yeet((uid, comp));
            comp.NextUpdate = comp.PerThrow + _timing.CurTime;
            Dirty(uid, comp);
        }
    }

    private void OnMapInit(Entity<TentacleHookedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = ent.Comp.PerThrow + _timing.CurTime;
        Dirty(ent);
    }

    private bool CheckDistance(Entity<TentacleHookedComponent> target)
    {
        if (target.Comp.ThrowTowards is not {} throwTowards)
            return false;

        var throwToPos = _transform.GetWorldPosition(throwTowards);
        var targetPos = _transform.GetWorldPosition(target.Owner);

        if ((throwToPos - targetPos).Length() <= target.Comp.MaxDistance)
        {
            PredictedQueueDel(target.Comp.Projectile);
            RemCompDeferred<TentacleHookedComponent>(target.Owner);
            return false;
        }

        return true;
    }

    private void Yeet(Entity<TentacleHookedComponent> target)
    {
        if (target.Comp.ThrowTowards is not {} throwTowards)
            return;

        var targetPos = _transform.GetWorldPosition(target.Owner);
        var throwToPos = _transform.GetWorldPosition(throwTowards);

        var dir = (throwToPos - targetPos).Normalized();

        _throwing.TryThrow(target, dir, target.Comp.ThrowStrength);
    }
}
