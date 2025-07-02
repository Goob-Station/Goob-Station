using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles healing or dealing damage to an entity that is standing on a lighted area.
/// </summary>
public sealed class LightDetectionDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightDetectionDamageComponent, ComponentStartup>(OnComponentStartup);
    }

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

    private void OnComponentStartup(EntityUid uid, LightDetectionDamageComponent component, ComponentStartup args)
    {
        if (component.ShowAlert)
            _alerts.ShowAlert(uid, component.AlertProto);

        component.DetectionValue = component.DetectionValueMax;
    }

    public void AddResistance(LightDetectionDamageComponent component, float amount)
    {
        component.ResistanceModifier += amount;
    }
}
