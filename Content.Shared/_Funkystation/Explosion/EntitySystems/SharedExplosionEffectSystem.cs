using Content.Shared._Funkystation.Explosion.Components;
using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Shared._Funkystation.Explosion.EntitySystems;

public abstract partial class SharedExplosionEffectSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExplosionEffectComponent, ExplosiveTriggeredEvent>(OnExplosionEffectTriggered);
    }

    private void OnExplosionEffectTriggered(Entity<ExplosionEffectComponent> ent, ref ExplosiveTriggeredEvent args)
    {
        var coords = _transform.GetMapCoordinates(ent);
        if (coords.MapId == Robust.Shared.Map.MapId.Nullspace)
            return;

        foreach (var effect in ent.Comp.VisualEffects)
        {
            Spawn(effect, coords);
        }

        if (ent.Comp.MaxShrapnel > 0)
        {
            foreach (var effect in ent.Comp.ShrapnelEffects)
            {
                var shrapnelCount = _random.Next(ent.Comp.MinShrapnel, ent.Comp.MaxShrapnel);
                for (var i = 0; i < shrapnelCount; i++)
                {
                    var angle = _random.NextAngle();
                    var direction = angle.ToVec().Normalized() * 10;
                    var shrapnel = Spawn(effect, coords);
                    if (Exists(shrapnel))
                    {
                        _throwing.TryThrow(shrapnel, direction, ent.Comp.ShrapnelSpeed / 10);
                    }
                }
            }
        }
    }
}
