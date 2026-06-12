using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Spawn cool effect, add reflective comp, remove it.
/// </summary>
public sealed class ReflectiveThreadsSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReflectiveThreadsComponent, ReflectiveThreadsActionEvent>(OnAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ReflectiveThreadsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Reflecting)
                continue;

            comp.Accumulator += frameTime;

            if (comp.Accumulator < comp.ReflectDuration)
                continue;

            comp.Accumulator = 0f;
            comp.Reflecting = false;

            RemCompDeferred<ReflectComponent>(uid);

            if (comp.EffectEntity.HasValue)
            {
                QueueDel(comp.EffectEntity.Value);
                comp.EffectEntity = null;
            }
        }
    }

    private void OnAction(EntityUid uid, ReflectiveThreadsComponent comp, ReflectiveThreadsActionEvent args)
    {
        if (args.Handled || comp.Reflecting)
            return;

        _audio.PlayPredicted(comp.ReflectSound, uid, uid);

        comp.EffectEntity = PredictedSpawnAttachedTo(comp.EffectPrototype, Transform(uid).Coordinates);
        if (comp.EffectEntity.HasValue)
        {
            _transform.SetParent(comp.EffectEntity.Value, uid);
        }

        var reflect = EnsureComp<ReflectComponent>(uid);
        reflect.ReflectProb = 1f;
        reflect.Reflects = ReflectType.Energy | ReflectType.NonEnergy | ReflectType.Magic;

        comp.Reflecting = true;
        comp.Accumulator = 0f;
        args.Handled = true;
    }
}
