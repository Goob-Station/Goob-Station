using Robust.Shared.Map;

namespace Content.Goobstation.Server.SpaceWhale; // predictions? how bout you predict my ass, but seriously this is THE problem with ts cuz i have no fucking idea how to predict it
// edit ok nvm it looks sorta fine with mobs but please do not put this on something that is predicted otherwise it will look like shit

public sealed class TailedEntitySystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TailedEntityComponent, ComponentShutdown>(OnComponentShutdown);
    }


    private void OnComponentShutdown(EntityUid uid, TailedEntityComponent component, ComponentShutdown args)
    {
        foreach (var segment in component.TailSegments)
            QueueDel(segment);

        component.TailSegments.Clear();
    }

    public override void Update(float frameTime)
    {
        var eqe = EntityQueryEnumerator<TailedEntityComponent, TransformComponent>();
        while (eqe.MoveNext(out var uid, out var comp, out var xform))
        {

            if (comp.TailSegments.Count == 0)
            {
                InitializeTailSegments(uid, comp, xform);
                continue; // its needed because it fucking crashes lmao
            }

            UpdateTailPositions((uid, comp, xform), frameTime);
        }
    }

    private void InitializeTailSegments(EntityUid uid, TailedEntityComponent comp, TransformComponent xform)
    {
        var mapUid = xform.MapUid;
        if (mapUid == null)
            return;

        var headPos = _transformSystem.GetWorldPosition(xform);
        var headRot = _transformSystem.GetWorldRotation(xform);

        for (var i = 0; i < comp.Amount; i++)
        {
            var offset = headRot.ToWorldVec() * comp.Spacing * (i + 1);
            var spawnPos = headPos - offset;

            var segment = Spawn(comp.Prototype, new EntityCoordinates(mapUid.Value, spawnPos));
            comp.TailSegments.Add(segment);
        }
    }

    private void UpdateTailPositions(Entity<TailedEntityComponent, TransformComponent> ent, float frameTime)
    {
        var (uid, comp, xform) = ent;

        var headPos = _transformSystem.GetWorldPosition(xform);
        var headRot = _transformSystem.GetWorldRotation(xform);

        for (var i = 0; i < comp.TailSegments.Count; i++) // This is total goida, foreach is cleaner but i is needed in the loop
        {
            var segment = comp.TailSegments[i];
            if (!Exists(segment)
                || !TryComp(segment, out TransformComponent? segmentXform))
                continue;

            var offset = headRot.ToWorldVec() * comp.Spacing * (i + 1);
            var targetPos = headPos - offset;

            var currentPos = _transformSystem.GetWorldPosition(segmentXform);

            var diff = targetPos - currentPos;
            var distance = diff.Length();

            // ff close enough snap to position
            if (distance < comp.Spacing * 0.1f)
                _transformSystem.SetWorldPosition(segment, targetPos);
            else // Move toward target
            {
                var direction = diff.Normalized();
                var moveAmount = comp.Speed * frameTime;
                var moveDistance = MathF.Min(moveAmount, distance);
                var newPos = currentPos + direction * moveDistance;
                _transformSystem.SetWorldPosition(segment, newPos);
            }
        }

        //rotation shit
        for (var i = 0; i < comp.TailSegments.Count; i++)
        {
            var segment = comp.TailSegments[i];
            if (!Exists(segment)
                || !TryComp(segment, out TransformComponent? segmentXform))
                continue;

            var targetAngle = new Angle();

            if (i == 0)
            {// first segment should look at the head because there isnt a segment to look for
                var segmentPos = _transformSystem.GetWorldPosition(segmentXform);
                var direction = headPos - segmentPos;
                targetAngle = direction.ToWorldAngle();
            }
            else
            {// while other segments should look towards other segments
                var prevSegment = comp.TailSegments[i - 1];
                if (TryComp(prevSegment, out TransformComponent? prevXform))
                {
                    var segmentPos = _transformSystem.GetWorldPosition(segmentXform);
                    var prevPos = _transformSystem.GetWorldPosition(prevXform);
                    var direction = prevPos - segmentPos;
                    targetAngle = direction.ToWorldAngle();
                }
                else
                {
                    targetAngle = _transformSystem.GetWorldRotation(segmentXform);
                }
            }

            var curRot = _transformSystem.GetWorldRotation(segmentXform);
            var newRot = Angle.Lerp(curRot, targetAngle, comp.Speed * frameTime * 2f);
            _transformSystem.SetWorldRotation(segment, newRot);
        }
    }
}
