using Content.Pirate.Shared.Vampire.Components;
using Content.Shared.Damage;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Flash;
using Content.Shared.Humanoid;
using Content.Shared.Light.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Server.Light.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Pirate.Server.Vampire.Systems;

public sealed class ShadowSnareSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedEnsnareableSystem _ensnare = default!;
    [Dependency] private readonly SharedFlashSystem _flash = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLightSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowSnareComponent, StepTriggerAttemptEvent>(OnShadowSnareStepAttempt);
        SubscribeLocalEvent<ShadowSnareComponent, StepTriggeredOffEvent>(OnShadowSnareTriggered);
        SubscribeLocalEvent<ShadowSnareComponent, AfterFlashedEvent>(OnShadowSnareFlashed);
    }

    private void OnShadowSnareStepAttempt(EntityUid uid, ShadowSnareComponent component, ref StepTriggerAttemptEvent args)
        => args.Continue = true;

    private void OnShadowSnareTriggered(EntityUid uid, ShadowSnareComponent component, ref StepTriggeredOffEvent args)
    {
        var target = args.Tripper;

        // Only trigger on humanoids
        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        // Don't trigger on vampires or thralls
        if (HasComp<VampireComponent>(target) || HasComp<VampireThrallComponent>(target))
            return;

        // Apply brute damage
        _damageable.TryChangeDamage(target, component.Damage, true, origin: uid);

        // Apply temporary blindness using flash system
        var blindDuration = TimeSpan.FromSeconds(component.BlindDuration);
        _flash.Flash(target, null, null, blindDuration, slowTo: 1f, displayPopup: false);

        // Extinguish nearby lights
        ExtinguishNearbyLights(uid, component.LightExtinguishRadius);

        // Spawn ensnare entity and apply to target
        var ensnareEnt = Spawn(component.EnsnarePrototype, Transform(target).Coordinates);
        if (TryComp<EnsnaringComponent>(ensnareEnt, out var ensnaring))
        {
            ensnaring.WalkSpeed = component.WalkSpeed;
            ensnaring.SprintSpeed = component.SprintSpeed;
            ensnaring.FreeTime = component.FreeTime;
            ensnaring.BreakoutTime = component.BreakoutTime;
            _ensnare.TryEnsnare(target, ensnareEnt, ensnaring);
        }

        // Play trigger sound
        _audio.PlayPvs(component.TriggerSound, uid, AudioParams.Default.WithVolume(1f));

        QueueDel(uid);
    }

    private void OnShadowSnareFlashed(EntityUid uid, ShadowSnareComponent component, ref AfterFlashedEvent args)
        => QueueDel(uid);

    private void ExtinguishNearbyLights(EntityUid uid, float radius)
    {
        var center = Transform(uid).Coordinates;

        foreach (var ent in _lookup.GetEntitiesInRange(center, radius))
        {
            if (TryComp<PoweredLightComponent>(ent, out var light))
                _poweredLightSystem.SetState(ent, false, light);
        }
    }
}
