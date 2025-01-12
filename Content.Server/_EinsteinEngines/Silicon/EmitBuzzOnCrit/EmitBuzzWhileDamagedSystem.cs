using Content.Server.Popups;
using Content.Shared._EinsteinEngines.Silicon.EmitBuzzWhileDamaged;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Components;

namespace Content.Server._EinsteinEngines.Silicon.EmitBuzzOnCrit;

/// <summary>
/// This handles the buzzing popup and sound of a silicon based race when it is pretty damaged.
/// </summary>
public sealed class EmitBuzzWhileDamagedSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmitBuzzWhileDamagedComponent, MobStateComponent, MobThresholdsComponent, DamageableComponent>();

        while (query.MoveNext(out var uid, out var emitBuzzOnCritComponent, out var mobStateComponent, out var thresholdsComponent, out var damageableComponent))
        {

            if (_mobState.IsDead(uid, mobStateComponent)
                || !_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var threshold, thresholdsComponent)
                || damageableComponent.TotalDamage < threshold / 2)
                continue;

            emitBuzzOnCritComponent.AccumulatedFrametime += frameTime;

            if (emitBuzzOnCritComponent.AccumulatedFrametime < emitBuzzOnCritComponent.CycleDelay)
                continue;

            emitBuzzOnCritComponent.AccumulatedFrametime -= emitBuzzOnCritComponent.CycleDelay;

            if (_gameTiming.CurTime <= emitBuzzOnCritComponent.LastBuzzPopupTime + emitBuzzOnCritComponent.BuzzPopupCooldown)
                continue;

            // Start buzzing
            emitBuzzOnCritComponent.LastBuzzPopupTime = _gameTiming.CurTime;
            _popupSystem.PopupEntity(Loc.GetString("silicon-behavior-buzz"), uid);
            Spawn("EffectSparks", Transform(uid).Coordinates);
            _audio.PlayPvs(emitBuzzOnCritComponent.Sound, uid, AudioHelpers.WithVariation(0.05f, _robustRandom));
        }
    }

}
