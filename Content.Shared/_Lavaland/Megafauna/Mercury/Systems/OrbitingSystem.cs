using Content.Shared._Lavaland.Megafauna.Components;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed class OrbitingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OrbitingComponent>();
        while (query.MoveNext(out var uid, out var orbit))
        {
            var parent = Transform(uid).ParentUid;
            if (!Exists(parent))
            {
                QueueDel(uid);
                continue;
            }

            orbit.Radius = MathF.Min(orbit.Radius + orbit.GrowSpeed * frameTime, orbit.MaxRadius);
            orbit.Angle += MathF.Tau * frameTime;

            _xform.SetLocalPosition(uid, new Vector2(MathF.Cos(orbit.Angle), MathF.Sin(orbit.Angle)) * orbit.Radius);
        }
    }
}
