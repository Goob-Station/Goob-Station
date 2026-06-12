using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// Spawn a ring of orbiting entities
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

    private void OnAction(EntityUid uid, OrbitingRingComponent comp, OrbitingRingActionEvent args)
    {
        if (args.Handled)
            return;

        SpawnRing(uid, comp);
        args.Handled = true;
    }

    public void SpawnRing(EntityUid uid, OrbitingRingComponent comp)
    {
        // Delete previos rings so it doesn't get messy
        foreach (var ent in comp.Entities)
        {
            if (Exists(ent))
            {
                QueueDel(ent);
            }
        }
        comp.Entities.Clear();

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < comp.Count; i++)
        {
            var angle = MathF.Tau * i / comp.Count;
            var ent = SpawnAttachedTo(comp.Prototype, coords);
            _transform.SetParent(ent, uid);

            var orbit = EnsureComp<OrbitingComponent>(ent);
            orbit.Angle = angle;
            orbit.Radius = 0f;
            orbit.MaxRadius = comp.RingDistance;
            orbit.GrowSpeed = comp.GrowSpeed;
        }

        if (comp.Sound is not null)
        {
            _audio.PlayPvs(comp.Sound, uid);
        }
    }
}
