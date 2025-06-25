using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Server.Temperature.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Icy Veins logic. An AOE ability that lowers the temperature
/// of targets nearby and paralyzes them for a very short amount.
/// </summary>
public sealed class ShadowlingIcyVeinsSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingIcyVeinsComponent, IcyVeinsEvent>(OnIcyVeins);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        // Target's temperature adjusts to normal in a minute or so after the effect.
        // That means, they won't take lots of damage from this ability but they will be slowed down.
        var query = EntityQueryEnumerator<IcyVeinsTargetComponent>();
        while (query.MoveNext(out var uid, out var target))
        {
            if (TryComp<TemperatureComponent>(uid, out var temp))
            {
                target.Timer -= frameTime;

                if (target.Timer <= 0)
                {
                    temp.CurrentTemperature -= target.DecreaseTempByValue;
                    if (temp.CurrentTemperature <= target.MinTargetTemperature)
                        RemComp<IcyVeinsTargetComponent>(uid);

                    target.Timer = target.TimeUntilNextDecrease;
                }
            }
        }
    }

    private void OnIcyVeins(EntityUid uid, ShadowlingIcyVeinsComponent component, IcyVeinsEvent args)
    {
        foreach (var target in _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(args.Performer), component.Range))
        {
            TryIcyVeins(target, component);
        }

        var effectEnt = Spawn(component.IcyVeinsEffect, _transform.GetMapCoordinates(uid));
        _transform.SetParent(effectEnt, uid);

        _audio.PlayPvs(component.IcyVeinsSound, uid, AudioParams.Default.WithVolume(-1f));

        _actions.StartUseDelay(args.Action);
    }

    private void TryIcyVeins(EntityUid target, ShadowlingIcyVeinsComponent component)
    {

        if (!HasComp<MobStateComponent>(target))
            return;

        if (HasComp<ShadowlingComponent>(target) || HasComp<ThrallComponent>(target))
            return;

        if (!HasComp<TemperatureComponent>(target))
            return;

        EnsureComp<IcyVeinsTargetComponent>(target);
        _popup.PopupEntity(Loc.GetString("shadowling-icy-veins-activated"), target, target, PopupType.MediumCaution);

        // Paralyzing Checks
        if (HasComp<PsionicInsulationComponent>(target)) // this ability is broken so this may help a little
            return;

        if (!TryComp<StatusEffectsComponent>(target, out var statusEffectsComponent))
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(component.ParalyzeTime), false, statusEffectsComponent);
    }
}
