using System.Numerics;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared._White.Grab;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sandevistan;

public sealed partial class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ThrownItemSystem _thrownItem = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    // TODO: Sandevistan, Uncomment once the cinematic system is merged.
    // [Dependency] private readonly SharedCinematicSystem _cinematic = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));

        SubscribeLocalEvent<SandevistanUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanUserComponent, ToggleSandevistanEvent>(OnToggle);
        SubscribeLocalEvent<SandevistanUserComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SandevistanUserComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<SandevistanUserComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SandevistanUserComponent, GetDoAfterDelayMultiplierEvent>(OnModifyDoAfterDelay);
        SubscribeLocalEvent<SandevistanUserComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);

        InitializeDash();
        InitializeSlowfield();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateSlowfield();
        UpdateDash();

        if (_netManager.IsServer)
        {
            var glitchQuery = EntityQueryEnumerator<SandevistanGlitchComponent>();
            while (glitchQuery.MoveNext(out var glitchUid, out var glitchComp))
            {
                if (_timing.CurTime >= glitchComp.ExpiresAt)
                    RemCompDeferred<SandevistanGlitchComponent>(glitchUid);
            }

        }

        var inactiveQuery = EntityQueryEnumerator<SandevistanUserComponent>();
        while (inactiveQuery.MoveNext(out var inactiveUid, out var inactiveComp))
        {
            if (inactiveComp.Active || inactiveComp.CurrentLoad <= 0f)
                continue;

            inactiveComp.CurrentLoad = MathF.Max(0f, inactiveComp.CurrentLoad + inactiveComp.LoadPerInactiveSecond * frameTime);
            Dirty(inactiveUid, inactiveComp);
        }

        var query = EntityQueryEnumerator<ActiveSandevistanUserComponent, SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out _, out var comp))
        {
            if (!comp.Active)
                continue;

            UpdateAfterimages(uid, comp);

            // The load buildup is paused during the dash-attack animation.
            if (comp.DashActive)
                continue;

            comp.CurrentLoad += comp.LoadPerActiveSecond * frameTime;
            Dirty(uid, comp);

            var popup = -1;
            for (var i = (int) SandevistanState.Death; i >= 0; i--)
            {
                var state = (SandevistanState) i;
                if (!comp.Thresholds.TryGetValue(state, out var threshold) || comp.CurrentLoad < threshold)
                    continue;

                if (comp.Effects.TryGetValue(state, out var effects))
                    foreach (var effect in effects)
                        effect.Effect(uid, comp, EntityManager, frameTime);

                if (popup == -1 && state <= SandevistanState.Damage)
                    popup = i;
            }

            if (popup == -1 || comp.NextPopupTime > _timing.CurTime)
                continue;

            if (_netManager.IsServer)
                _popup.PopupEntity(Loc.GetString("sandevistan-overload-" + popup), uid, uid);

            comp.NextPopupTime = _timing.CurTime + comp.PopupDelay;
            Dirty(uid, comp);
        }
    }

    private void OnInit(Entity<SandevistanUserComponent> ent, ref ComponentInit args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.LoadAlert);
        Dirty(ent);
    }

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        args.Handled = true;

        if (ent.Comp.Active)
        {
            PlayToggleSound(ent, ent.Comp.EndSound);
            Disable(ent, ent.Comp);
            return;
        }

        Enable(ent);
    }

    /// <summary>
    /// Whether the sandevistan has enough load overhead to be enabled.
    /// </summary>
    private bool CanEnable(Entity<SandevistanUserComponent> ent)
    {
        if (!ent.Comp.Thresholds.TryGetValue(SandevistanState.Disable, out var max)
            && !ent.Comp.Thresholds.TryGetValue(SandevistanState.DisableNoAnim, out max))
            return true;

        var loadAfter = ent.Comp.CurrentLoad + ent.Comp.LoadPerActivation + ent.Comp.ActivationHeadroom;
        if (loadAfter < max.Float())
            return true;

        _popup.PopupClient(Loc.GetString("sandevistan-cooldown-popup"), ent, ent, PopupType.MediumCaution);
        return false;
    }

    /// <summary>
    /// Attempts to activate the sandevistan.
    /// </summary>
    private bool Enable(Entity<SandevistanUserComponent> ent)
    {
        if (ent.Comp.Active)
            return true;

        if (!CanEnable(ent))
            return false;

        ent.Comp.Active = true;
        EnsureComp<ActiveSandevistanUserComponent>(ent);

        ent.Comp.CurrentLoad += ent.Comp.LoadPerActivation;

        if (TryComp<SandevistanSlowedComponent>(ent, out var slowed))
        {
            var ev = new RemoveSandevistanSlowdownEvent(slowed.Source);
            RaiseLocalEvent(ent, ref ev);
        }

        _speed.RefreshMovementSpeedModifiers(ent);

        EntityManager.AddComponents(ent, ent.Comp.VisionComponents);
        EntityManager.AddComponents(ent, ent.Comp.ActivationComponents);

        SetFixtures(ent, ent.Comp, true);
        PlayToggleSound(ent, ent.Comp.StartSound);
        Dirty(ent);
        PlayLoopedAudio(ent, ent.Comp);
        return true;
    }

    private void OnRefreshSpeed(Entity<SandevistanUserComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
            args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private void OnMeleeAttack(Entity<SandevistanUserComponent> ent, ref MeleeAttackEvent args)
    {
        if (!ent.Comp.Active
            || !TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon))
            return;

        var rate = weapon.NextAttack - _timing.CurTime; //weapon.AttackRate; breaks things when multiple systems modify NextAttack
        weapon.NextAttack -= rate - rate / ent.Comp.AttackSpeedModifier;
    }

    private void OnModifyDoAfterDelay(Entity<SandevistanUserComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        if (ent.Comp.Active)
            args.Multiplier *= ent.Comp.DoAfterModifier;
    }

    private void OnBeforeStaminaDamage(Entity<SandevistanUserComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (ent.Comp.Active
            && args.Source == ent.Owner)
            args.Cancelled = true;
    }

    private void OnMobStateChanged(Entity<SandevistanUserComponent> ent, ref MobStateChangedEvent args) =>
        Disable(ent, ent.Comp);

    private void OnShutdown(Entity<SandevistanUserComponent> ent, ref ComponentShutdown args)
    {
        Disable(ent, ent.Comp);
        _alerts.ClearAlert(ent.Owner, ent.Comp.LoadAlert);
    }

    public void Disable(EntityUid uid, SandevistanUserComponent comp)
    {
        EndDash(uid, comp, disable: false);

        var wasActive = comp.Active;
        if (comp.Active)
        {
            SetFixtures(uid, comp, false);

            // Remove slowdown from all affected entities
            var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
            while (query.MoveNext(out var target, out var slowed))
            {
                if (slowed.Source != uid)
                    continue;

                var ev = new RemoveSandevistanSlowdownEvent(uid);
                RaiseLocalEvent(target, ref ev);
            }

            RemCompDeferred<ActiveSandevistanUserComponent>(uid);
            comp.Active = false;
        }

        comp.ColorAccumulator = 0;
        _speed.RefreshMovementSpeedModifiers(uid);
        comp.PlayingStream = _audio.Stop(comp.PlayingStream);

        EntityManager.RemoveComponents(uid, comp.VisionComponents);
        EntityManager.RemoveComponents(uid, comp.ActivationComponents);

        if (wasActive)
            Dirty(uid, comp);
    }

    #region Afterimage Methods

    /// <summary>
    /// Update afterimages for sandevistan user
    /// </summary>
    public void UpdateAfterimages(EntityUid uid, SandevistanUserComponent comp)
    {
        if (_netManager.IsServer || !_timing.IsFirstTimePredicted)
            return;

        var pos = _transform.GetWorldPosition(uid);

        var stale = _timing.CurTime - comp.LastAfterimageTrackTime > comp.TrailRestartGap;
        comp.LastAfterimageTrackTime = _timing.CurTime;

        var delta = pos - comp.LastAfterimagePos;
        var dist = delta.Length();

        if (stale || dist > comp.AfterimageDistance * comp.MaxAfterimagesPerUpdate)
        {
            SpawnAfterimage(uid, comp, pos);
            comp.ColorAccumulator++;
            comp.LastAfterimagePos = pos;
            return;
        }

        if (dist < comp.AfterimageDistance)
            return;

        var dir = delta / dist;
        var steps = (int) (dist / comp.AfterimageDistance);
        for (var i = 1; i <= steps; i++)
        {
            SpawnAfterimage(uid, comp, comp.LastAfterimagePos + dir * (comp.AfterimageDistance * i));
            comp.ColorAccumulator++;
        }

        comp.LastAfterimagePos += dir * (comp.AfterimageDistance * steps);
    }

    /// <summary>
    /// Spawn an afterimage for a sandevistan user at the given world position.
    /// </summary>
    private void SpawnAfterimage(EntityUid uid, SandevistanUserComponent comp, Vector2 worldPos)
    {
        var xform = Transform(uid);
        var coordinates = xform.ParentUid.IsValid()
            ? _transform.ToCoordinates(xform.ParentUid, new MapCoordinates(worldPos, xform.MapID))
            : xform.Coordinates;

        var afterimage = Spawn(null, coordinates);

        // Can't use ensurecomp due to prediction shenanigans.
        // This just makes sure the component has these fields changed before being added.
        AddComp(afterimage, new SandevistanAfterimageComponent
        {
            SourceEntity = uid,
            Color = comp.AfterimageColor ?? Color.FromHsv(new Vector4(comp.ColorAccumulator % 100f / 100f, 1f, 1f, 1f)),
            DirectionOverride = xform.LocalRotation.GetCardinalDir(),
            Order = comp.ColorAccumulator,
        });
    }

    #endregion

    #region Audio Methods
    /// <summary>
    /// Play looped audio for sandevistan user
    /// </summary>
    public void PlayLoopedAudio(EntityUid uid, SandevistanUserComponent comp)
    {
        if (!_netManager.IsServer || comp.LoopSound == null || comp.PlayingStream != null)
            return;

        Timer.Spawn(TimeSpan.FromSeconds(comp.LoopSoundDelay), () =>
        {
            if (!TerminatingOrDeleted(uid) && comp.Active && comp.PlayingStream == null)
            {
                var stream = _audio.PlayPvs(comp.LoopSound, uid);
                if (stream?.Entity is { } entity)
                    comp.PlayingStream = entity;
            }
        });
    }
    /// <summary>
    /// Plays an activation/deactivation sound.
    /// </summary>
    private void PlayToggleSound(Entity<SandevistanUserComponent> ent, SoundSpecifier? sound)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        _audio.Stop(ent.Comp.ToggleStream);
        ent.Comp.ToggleStream = _audio.PlayPredicted(sound, ent, ent)?.Entity;
    }
    #endregion
}
