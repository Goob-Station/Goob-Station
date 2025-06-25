using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles healing or dealing damage to an entity that is standing on a lighted area.
/// </summary>
public sealed class LightDetectionDamageModifierSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightDetectionDamageModifierComponent, ComponentStartup>(OnComponentStartup);
    }
    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);


        var query = EntityQueryEnumerator<LightDetectionDamageModifierComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!TryComp<LightDetectionComponent>(uid, out var lightDet))
                continue;

            if (comp.ShowAlert)
            {
                Dirty(uid, comp);
                _alerts.ShowAlert(uid, comp.AlertProto);
            }

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            if (lightDet.IsOnLight)
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

            if (comp.DetectionValue <= 0 && comp.TakeDamageOnLight)
            {
                // Take Damage
                if (_timing.CurTime >= comp.NextUpdateDamage)
                {
                    // Prevents from ashing when critical
                    if (!_mobState.IsCritical(uid))
                    {
                        _damageable.TryChangeDamage(uid, comp.DamageToDeal * comp.ResistanceModifier);
                        _audio.PlayPvs(
                            new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/energy_meat1.ogg"),
                            uid,
                            AudioParams.Default.WithVolume(-2f));
                        comp.NextUpdateDamage = _timing.CurTime + comp.DamageInterval;
                    }
                }
            }
            else if (comp.DetectionValue >= comp.DetectionValueMax && comp.HealOnShadows)
            {
                // Heal Damage
                if (_timing.CurTime >= comp.NextUpdateHeal)
                {
                    _damageable.TryChangeDamage(uid, comp.DamageToHeal);
                    comp.NextUpdateHeal = _timing.CurTime + comp.HealInterval;
                }
            }
            comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;
        }
    }

    private void OnComponentStartup(
        EntityUid uid,
        LightDetectionDamageModifierComponent component,
        ComponentStartup args
    )
    {
        component.NextUpdate       = _timing.CurTime;
        component.NextUpdateDamage = _timing.CurTime;
        component.NextUpdateHeal   = _timing.CurTime;
        component.DetectionValue   = component.DetectionValueMax;
    }

    public void AddResistance(float amount, EntityUid uid, LightDetectionDamageModifierComponent component)
    {
        component.ResistanceModifier += amount;
    }
}
