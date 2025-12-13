using Robust.Shared.Map;

namespace Content.Goobstation.Server.SpaceWhale // predictions? how bout you predict my ass, but seriously this is THE problem with ts cuz i have no fucking idea how to predict it
// edit ok nvm it looks sorta fine with mobs but please do not put this on something that is predicted otherwise it will look like shit
{
    public sealed class TailedEntitySystem : EntitySystem
    {
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        private readonly HashSet<EntityUid> _activeTails = new();

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<TailedEntityComponent, ComponentStartup>(OnComponentStartup);
            SubscribeLocalEvent<TailedEntityComponent, ComponentShutdown>(OnComponentShutdown);
        }

        private void OnComponentStartup(EntityUid uid, TailedEntityComponent component, ComponentStartup args)
        {
            // wait until the entity is fully initialized before spawning tail segments
            // this will happen in the first Update thingy
            _activeTails.Add(uid);
        }

        private void OnComponentShutdown(EntityUid uid, TailedEntityComponent component, ComponentShutdown args)
        {
            foreach (var segment in component.TailSegments)
            {
                if (Exists(segment))
                    QueueDel(segment);
            }
            component.TailSegments.Clear();

            _activeTails.Remove(uid);
        }

        public override void Update(float frameTime)
        {
            foreach (var uid in _activeTails)
            {
                if (!TryComp(uid, out TailedEntityComponent? comp)
                    || !TryComp(uid, out TransformComponent? xform))
                {
                    _activeTails.Remove(uid);
                    continue;
                }

                if (comp.TailSegments.Count == 0)
                {
                    InitializeTailSegments(uid, comp, xform);
                    continue; // its needed because it fucking crashes lmao
                }

                UpdateTailPositions(uid, comp, xform, frameTime);
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

        private void UpdateTailPositions(EntityUid uid, TailedEntityComponent comp, TransformComponent xform, float frameTime)
        {
            var headPos = _transformSystem.GetWorldPosition(xform);
            var headRot = _transformSystem.GetWorldRotation(xform);

            for (var i = 0; i < comp.TailSegments.Count; i++)
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
        }
    }
}
