using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// Spawn a ring of orbiting entities.
/// </summary>
public sealed class OrbitingRingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrbitingRingComponent, OrbitingRingActionEvent>(OnAction);
    }

    private void OnAction(Entity<OrbitingRingComponent> ent, ref OrbitingRingActionEvent args)
    {
        if (args.Handled)
            return;

        var distance = args.RingDistance > 0f ? args.RingDistance : ent.Comp.RingDistance;

        SpawnRing(ent.Owner, ent.Comp, distance, args.RingId);
        args.Handled = true;
    }

    public void SpawnRing(EntityUid uid, OrbitingRingComponent comp)
    {
        SpawnRing(uid, comp, comp.RingDistance, string.Empty);
    }

    public void SpawnRing(EntityUid uid, OrbitingRingComponent comp, float ringDistance, string ringId)
    {
        if (comp.Rings.TryGetValue(ringId, out var existing))
        {
            foreach (var ent in existing)
            {
                if (Exists(ent))
                {
                    QueueDel(ent);
                }
            }

            existing.Clear();
        }
        else
        {
            existing = new List<EntityUid>();
            comp.Rings[ringId] = existing;
        }

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < comp.Count; i++)
        {
            var angle = MathF.Tau * i / comp.Count;
            var ent = SpawnAttachedTo(comp.Prototype, coords);
            _transform.SetParent(ent, uid);

            var orbit = EnsureComp<OrbitingComponent>(ent);
            orbit.Angle = angle;
            orbit.Radius = 0f;
            orbit.MaxRadius = ringDistance;
            orbit.GrowSpeed = comp.GrowSpeed;

            existing.Add(ent);
        }

        if (comp.Sound is not null)
        {
            _audio.PlayPvs(comp.Sound, uid);
        }
    }
}
