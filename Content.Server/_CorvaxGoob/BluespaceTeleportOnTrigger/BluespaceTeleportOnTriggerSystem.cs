using Content.Server.Explosion.EntitySystems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._CorvaxGoob.BluespaceTeleportOnTriggerOnTrigger;

public sealed class BluespaceTeleportOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceTeleportOnTriggerComponent, TriggerEvent>(HandleTrigger);
    }

    private void HandleTrigger(Entity<BluespaceTeleportOnTriggerComponent> entity, ref TriggerEvent args)
    {
        var xform = Transform(entity);
        var mapPos = _xform.GetWorldPosition(xform);
        var radius = _random.Next(3, 10);
        var gridBounds = new Box2(mapPos - new Vector2(radius, radius), mapPos + new Vector2(radius, radius));
        var mobs = new HashSet<Entity<MobStateComponent>>();
        _lookup.GetEntitiesInRange(xform.Coordinates, entity.Comp.Range, mobs, flags: LookupFlags.Uncontained);

        foreach (var comp in mobs)
        {
            if (!_random.Prob(entity.Comp.Probability))
                return;

            var ent = comp.Owner;
            var randomX = _random.NextFloat(gridBounds.Left, gridBounds.Right);
            var randomY = _random.NextFloat(gridBounds.Bottom, gridBounds.Top);

            var pos = new Vector2(randomX, randomY);

            _xform.SetWorldPosition(ent, pos);
            _audio.PlayPvs(entity.Comp.TeleportSound, ent);
        }
    }
}
