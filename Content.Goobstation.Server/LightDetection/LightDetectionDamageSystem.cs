using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.LightDetection.Systems;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.LightDetection;

/// <summary>
/// This handles healing or dealing damage to an entity that is standing on a lighted area.
/// </summary>
public sealed class LightDetectionDamageSystem : SharedLightDetectionDamageSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightDetectionDamageComponent, LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp, out var lightDet))
        {
            comp.Accumulator -= frameTime;

            if (comp.TakeDamageOnLight)
                comp.DamageAccumulator -= frameTime;
            if (comp.HealOnShadows)
                comp.HealAccumulator -= frameTime;

            if (comp.Accumulator <= 0)
            {
                UpdateDetectionValues(comp, lightDet.IsOnLight);
                DirtyField(uid, comp, nameof(LightDetectionDamageComponent.DetectionValue));
                DirtyField(uid, lightDet, nameof(LightDetectionComponent.IsOnLight));
                comp.Accumulator = comp.UpdateInterval;
            }

            if (comp.DamageAccumulator <= 0
                && comp.DetectionValue <= 0
                && comp.TakeDamageOnLight
                && !_mobState.IsCritical(uid))
            {
                // Take Damage
                _damageable.TryChangeDamage(uid, comp.DamageToDeal * comp.ResistanceModifier);
                _audio.PlayPvs(comp.SoundOnDamage, uid, AudioParams.Default.WithVolume(-2f));
                comp.DamageAccumulator = comp.DamageInterval;
            }

            if (comp.HealAccumulator <= 0
                && comp.DetectionValue >= comp.DetectionValueMax
                && comp.HealOnShadows)
            {
                _damageable.TryChangeDamage(uid, comp.DamageToHeal);
                comp.HealAccumulator = comp.HealInterval;
            }
        }
    }

    private void UpdateDetectionValues(LightDetectionDamageComponent comp, bool onLight)
    {
        if (onLight)
        {
            comp.DetectionValue -= comp.DetectionValueFactor;

            if (comp.DetectionValue <= 0)
                comp.DetectionValue = 0;
        }
        else
        {
            comp.DetectionValue += comp.DetectionValueFactor;

            if (comp.DetectionValue >= comp.DetectionValueMax)
                comp.DetectionValue = comp.DetectionValueMax;
        }
    }
}
