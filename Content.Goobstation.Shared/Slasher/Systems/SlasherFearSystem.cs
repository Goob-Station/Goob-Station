using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Fluids;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the slashers jumpscares / music / fear meter / fear overlay / blood trail / etc.
/// </summary>
public sealed partial class SlasherFearSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherFearComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SlasherFearComponent, ComponentStartup>(OnFearStartup);
        SubscribeLocalEvent<SlasherFearComponent, ComponentShutdown>(OnFearShutdown);
        SubscribeLocalEvent<SlasherFearComponent, LocalPlayerDetachedEvent>(OnFearDetached);
        SubscribeLocalEvent<SlasherFearComponent, SlasherToggleFearMusicAlertEvent>(OnToggleMusic);

        SubscribeLocalEvent<SlasherVictimFearBuildupComponent, ComponentShutdown>(OnFearedShutdown);
        SubscribeLocalEvent<SlasherVictimFearBuildupComponent, LocalPlayerDetachedEvent>(OnFearedDetached);
    }

    private void OnRefreshSpeed(Entity<SlasherFearComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.IsSpeedBoostActive)
            return;

        var mod = 1f + ent.Comp.MaxSpeedBonus;
        args.ModifySpeed(mod, mod);
    }

    private void OnFearStartup(Entity<SlasherFearComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.SeenAlert, 0);
    }

    private void OnFearShutdown(Entity<SlasherFearComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.Alert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.SeenAlert);
        StopBloodTrail(ent.Owner);
        StopSlasherMusic(ent);
        ReleaseVictims(ent.Owner);
    }

    private void OnFearDetached(Entity<SlasherFearComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        StopSlasherMusic(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.IsFirstTimePredicted)
        {
            UpdateMusicFades(frameTime);
            UpdateObserverMusic();
        }

        var now = _timing.CurTime;

        var slashers = EntityQueryEnumerator<SlasherFearComponent>();
        while (slashers.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextCheck)
                continue;

            comp.NextCheck = now + comp.CheckInterval;
            Scan((uid, comp), now);
        }

        UpdateVictimFearBuildup(now);
        UpdateBloodTrails(now);
    }

    private void Scan(Entity<SlasherFearComponent> ent, TimeSpan now)
    {
        var (uid, comp) = ent;
        var seen = new HashSet<NetEntity>();

        foreach (var other in _lookup.GetEntitiesInRange(uid, comp.Range))
        {
            if (!IsValidVictim(uid, other)
                || !_interaction.InRangeUnobstructed(uid, other, comp.Range, CollisionGroup.Opaque))
                continue;

            var netOther = GetNetEntity(other);
            seen.Add(netOther);

            if (!CanHunt(uid))
                continue;

            if (!TryObserveVictim(ent, other, now, out var victim))
                continue;

            var isNewPerson = !comp.Observing.Contains(netOther);

            if (isNewPerson && now >= comp.NextJumpscare)
            {
                comp.NextJumpscare = now + comp.JumpscareCooldown;
                comp.CurrentMeter = MathF.Min(comp.MaxMeter, comp.CurrentMeter + comp.MeterPerJumpscare);
                victim.Fear = MathF.Min(1f, victim.Fear + comp.JumpscareFear);

                Jumpscare(uid, other, comp);
            }
            Dirty(other, victim);

        }

        var anySeen = seen.Count > 0;
        var dt = (float) comp.CheckInterval.TotalSeconds;

        SetObserved(ent, anySeen);

        if (!CanHunt(uid))
        {
            comp.Observing.Clear();
            comp.CurrentMeter = MathF.Max(0f, comp.CurrentMeter - comp.MeterDecayPerSecond * dt);
            Dirty(uid, comp);
            UpdateHuntEffects(ent);
            return;
        }

        if (anySeen)
        {
            comp.LastSeenVictim = now;
            comp.CurrentMeter = MathF.Min(comp.MaxMeter, comp.CurrentMeter + comp.MeterPassivePerSecond * dt);
        }
        else if (now - comp.LastSeenVictim >= comp.MeterGracePeriod)
        {
            comp.CurrentMeter = MathF.Max(0f, comp.CurrentMeter - comp.MeterDecayPerSecond * dt);
        }

        comp.Observing = seen;
        Dirty(uid, comp);
        UpdateHuntEffects(ent);
    }

    private void UpdateHuntEffects(Entity<SlasherFearComponent> ent)
    {
        var (uid, comp) = ent;

        var boosted = comp.CurrentMeter >= comp.MaxMeter;
        if (comp.IsSpeedBoostActive != boosted)
        {
            comp.IsSpeedBoostActive = boosted;
            Dirty(uid, comp);
            _movementSpeed.RefreshMovementSpeedModifiers(uid);
        }

        if (comp.CurrentMeter > 0f)
            _alerts.ShowAlert(uid, comp.Alert);
        else
            _alerts.ClearAlert(uid, comp.Alert);

        UpdateBloodTrailState(ent);
        UpdateMusic(ent);
    }

    private void SetObserved(Entity<SlasherFearComponent> ent, bool observed)
    {
        if (ent.Comp.IsObserved != observed)
        {
            ent.Comp.IsObserved = observed;
            Dirty(ent);
        }

        _alerts.ShowAlert(ent.Owner, ent.Comp.SeenAlert, (short) (observed ? 1 : 0));
    }

    public bool IsObservedByPlayers(EntityUid uid, float range)
    {
        foreach (var other in _lookup.GetEntitiesInRange(uid, range))
        {
            if (other == uid
                || !HasComp<EyeComponent>(other)
                || HasComp<GhostComponent>(other)
                || !HasComp<HumanoidAppearanceComponent>(other)
                || _mobState.IsDead(other)
                || _mobState.IsCritical(other)
                || TryComp<BlindableComponent>(other, out var blind) && blind.IsBlind
                || TryComp<SlasherIncorporealComponent>(other, out var otherSlasher) && otherSlasher.IsIncorporeal)
                continue;

            if (_interaction.InRangeUnobstructed(other, uid, range, CollisionGroup.Opaque))
                return true;
        }

        return false;
    }

    private bool CanHunt(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
            || TryComp<SlasherIncorporealComponent>(uid, out var inc) && inc.IsIncorporeal)
            return false;

        return true;
    }

    private bool IsValidVictim(EntityUid slasher, EntityUid other)
    {
        if (other == slasher
            || HasComp<SlasherComponent>(other)
            || !HasComp<EyeComponent>(other)
            || HasComp<GhostComponent>(other)
            || !HasComp<HumanoidAppearanceComponent>(other)
            || !_mind.TryGetMind(other, out _, out _)
            || _mobState.IsDead(other)
            || _mobState.IsCritical(other)
            || TryComp<BlindableComponent>(other, out var blind) && blind.IsBlind
            || TryComp<SlasherIncorporealComponent>(other, out var inc) && inc.IsIncorporeal)
            return false;

        return true;
    }
}
