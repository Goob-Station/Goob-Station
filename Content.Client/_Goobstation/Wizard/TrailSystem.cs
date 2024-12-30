using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.TimeStop;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Spawners;

namespace Content.Client._Goobstation.Wizard;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new TrailOverlay(EntityManager));

        SubscribeLocalEvent<TrailComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TrailComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(Entity<TrailComponent> ent, ref ComponentRemove args)
    {
        var (uid, comp) = ent;

        if (comp.TrailData.Count == 0 || comp.Frequency <= 0f || comp.Lifetime <= 0f)
            return;

        var remainingTrail = Spawn(null, _transform.GetMapCoordinates(uid));
        EnsureComp<TimedDespawnComponent>(remainingTrail).Lifetime = comp.Lifetime;
        var trail = EnsureComp<TrailComponent>(remainingTrail);
        trail.Frequency = 0f;
        trail.Lifetime = comp.Lifetime;
        trail.LerpAmount = comp.LerpAmount;
        trail.Sprite = comp.Sprite;
        trail.TrailData = comp.TrailData;
        trail.TrailData.Sort((x, y) => y.Color.A.CompareTo(x.Color.A));
    }

    private void OnStartup(Entity<TrailComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Accumulator = ent.Comp.Frequency;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<TrailOverlay>();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var xformQuery = GetEntityQuery<TransformComponent>();
        var frozenQuery = GetEntityQuery<FrozenComponent>();

        var query = EntityQueryEnumerator<TrailComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var trail, out var xform))
        {
            if (trail.Lifetime <= 0f)
                continue;

            if (frozenQuery.HasComp(uid))
                continue;

            if (trail.LerpAmount > 0f)
            {
                foreach (var data in trail.TrailData)
                {
                    data.Color = data.Color.WithAlpha(float.Lerp(data.Color.A, 0f, trail.LerpAmount));
                }
            }

            trail.Accumulator += frameTime;

            if (trail.Frequency <= 0f)
            {
                if (trail.Accumulator <= trail.Lifetime)
                    continue;

                trail.Accumulator = 0f;

                if (trail.TrailData.Count == 0f)
                {
                    if (IsClientSide(uid))
                        QueueDel(uid);
                    continue;
                }

                trail.TrailData.RemoveAt(trail.TrailData.Count - 1);

                continue;
            }

            if (trail.Accumulator <= trail.Frequency)
                continue;

            trail.Accumulator = 0f;

            var rotation = _transform.GetWorldRotation(xform, xformQuery);
            var position = _transform.GetWorldPosition(xform, xformQuery);

            if (trail.TrailData.Count < trail.Lifetime / trail.Frequency)
            {
                trail.TrailData.Add(new TrailData(position, rotation, Color.White));
            }
            else
            {
                if (trail.CurIndex >= trail.TrailData.Count)
                    trail.CurIndex = 0;

                trail.TrailData[trail.CurIndex].Color = Color.White;
                trail.TrailData[trail.CurIndex].Position = position;
                trail.TrailData[trail.CurIndex].Angle = rotation;

                trail.CurIndex++;
            }
        }
    }
}
