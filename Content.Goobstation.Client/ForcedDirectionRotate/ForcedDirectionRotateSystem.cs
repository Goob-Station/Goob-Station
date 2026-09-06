using Content.Goobstation.Shared.ForcedDirectionRotate;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ForcedDirectionRotate;

/// <summary>
/// Overrides the sprite direction of any entity.
/// </summary>
public sealed class ForcedDirectionRotateSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ForcedDirectionRotateComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ForcedDirectionRotateComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            if (comp.FaceTarget is { } target)
            {
                if (!Exists(target))
                    continue;

                var toTarget = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(uid);
                if (toTarget.LengthSquared() < 0.01f)
                    continue;

                sprite.EnableDirectionOverride = true;
                sprite.DirectionOverride = (-toTarget).ToWorldAngle().GetCardinalDir();
                continue;
            }

            if (comp.Directions.Count == 0)
                continue;

            if (!sprite.EnableDirectionOverride)
            {
                sprite.EnableDirectionOverride = true;
                sprite.DirectionOverride = comp.Directions[comp.Index % comp.Directions.Count];
            }

            if (_timing.CurTime < comp.NextChange)
                continue;

            comp.NextChange = _timing.CurTime + comp.Interval;
            comp.Index = (comp.Index + 1) % comp.Directions.Count;
            sprite.DirectionOverride = comp.Directions[comp.Index];
        }
    }

    private void OnShutdown(Entity<ForcedDirectionRotateComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            sprite.EnableDirectionOverride = false;
    }
}
