
using Content.Shared.Bed.Sleep;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared._EinsteinEngines.Flight;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;

namespace Content.Server._EinsteinEngines.Flight;
public sealed class FlightSystem : SharedFlightSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ToggleFlightEvent>(OnToggleFlight);
        SubscribeLocalEvent<FlightComponent, FlightDoAfterEvent>(OnFlightDoAfter);
        SubscribeLocalEvent<FlightComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<FlightComponent, EntityZombifiedEvent>(OnZombified);
        SubscribeLocalEvent<FlightComponent, KnockedDownEvent>(OnKnockedDown);
        SubscribeLocalEvent<FlightComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<FlightComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<FlightComponent, SleepStateChangedEvent>(OnSleep);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FlightComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.On)
                continue;

            component.TimeUntilFlap -= frameTime;

            if (component.TimeUntilFlap > 0f)
                continue;

            _audio.PlayPvs(component.FlapSound, uid);
            component.TimeUntilFlap = component.FlapInterval;

        }
    }

    #region Core Functions
    private void OnToggleFlight(EntityUid uid, FlightComponent component, ToggleFlightEvent args)
    {
        // If the user isnt flying, we check for conditionals and initiate a doafter.
        if (!component.On)
        {
            if (!CanFly(uid, component))
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager,
            uid, component.ActivationDelay,
            new FlightDoAfterEvent(), uid, target: uid)
            {
                BlockDuplicate = true,
                BreakOnDamage = true,
                NeedHand = true,
                MultiplyDelay = false, // Goobstation
            };

            if (!_doAfter.TryStartDoAfter(doAfterArgs))
                return;
        }
        else
            ToggleActive(uid, false, component);
    }

    private void OnFlightDoAfter(EntityUid uid, FlightComponent component, FlightDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        ToggleActive(uid, true, component);
        args.Handled = true;
    }

    #endregion

    #region Conditionals

    private bool CanFly(EntityUid uid, FlightComponent component)
    {
        if (TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract)
        {
            _popupSystem.PopupEntity(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
            return false;
        }

        if (HasComp<ZombieComponent>(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("no-flight-while-zombified"), uid, uid, PopupType.Medium);
            return false;
        }

        if (HasComp<StandingStateComponent>(uid) && _standing.IsDown(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("no-flight-while-lying"), uid, uid, PopupType.Medium);
            return false;
        }
        return true;
    }

    private void OnMobStateChangedEvent(EntityUid uid, FlightComponent component, MobStateChangedEvent args)
    {
        if (!component.On
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleActive(args.Target, false, component);
    }

    private void OnZombified(EntityUid uid, FlightComponent component, ref EntityZombifiedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(args.Target, false, component);
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;
        Dirty(uid, stamina);
    }

    private void OnKnockedDown(EntityUid uid, FlightComponent component, ref KnockedDownEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
    }

    private void OnStunned(EntityUid uid, FlightComponent component, ref StunnedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
    }

    private void OnDowned(EntityUid uid, FlightComponent component, ref DownedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
    }

    private void OnSleep(EntityUid uid, FlightComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.On
            || !args.FellAsleep)
            return;

        ToggleActive(uid, false, component);
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        Dirty(uid, stamina);
    }
    #endregion
}
