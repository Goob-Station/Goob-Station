// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Abilities;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sandevistan;

public sealed class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanUserComponent, ToggleSandevistanEvent>(OnToggle);
        SubscribeLocalEvent<SandevistanUserComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SandevistanUserComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<SandevistanUserComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SandevistanUserComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.ApplyingState)
           return;

        var query = EntityQueryEnumerator<SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.DisableAt != null
                && _timing.CurTime > comp.DisableAt)
                Disable(uid, comp);

            if (comp.Trail != null)
            {
                comp.Trail.Color = Color.FromHsv(new Vector4(comp.ColorAccumulator % 100f / 100f, 1, 1, 1));
                comp.ColorAccumulator++;
                Dirty(uid, comp.Trail);
            }

            if (comp.NextExecutionTime > _timing.CurTime)
                continue;

            comp.NextExecutionTime = _timing.CurTime + comp.UpdateDelay;

            if (!comp.Enabled)
            {
                _audio.Stop(comp.RunningSound);
                comp.CurrentLoad = MathF.Max(0, comp.CurrentLoad + comp.LoadPerInactiveSecond);
                continue;
            }

            comp.CurrentLoad += comp.LoadPerActiveSecond;

            var stateActions = new Dictionary<int, Action>
            {
                { 1, () => _jittering.DoJitter(uid, comp.StatusEffectTime, true)},
                { 2, () => _stamina.TakeStaminaDamage(uid, comp.StaminaDamage)},
                { 3, () => _damageable.TryChangeDamage(uid, comp.Damage)},
                { 4, () => _stun.TryKnockdown(uid, comp.StatusEffectTime, true)},
                { 5, () => Disable(uid, comp)},
                { 6, () => _mobState.ChangeMobState(uid, MobState.Dead)},
            };

            var filteredStates = new List<int>();
            foreach (var stateThreshold in comp.Thresholds)
                if (comp.CurrentLoad >= stateThreshold.Value)
                    filteredStates.Add((int)stateThreshold.Key);

            filteredStates.Sort((a, b) => b.CompareTo(a));
            foreach (var state in filteredStates)
                if (stateActions.TryGetValue(state, out var action))
                    action();

            if (comp.NextPopupTime > _timing.CurTime)
                continue;

            var popup = -1;
            foreach (var state in filteredStates)
            {
                if (state > 3) // Goida
                    continue;
                if (state > popup)
                    popup = state;
            }

            if (popup == -1)
                continue;

            _popup.PopupClient(Loc.GetString("sandevistan-overload-" + popup), uid, uid);
            comp.NextPopupTime = _timing.CurTime + comp.PopupDelay;
        }
    }

    private void OnInit(Entity<SandevistanUserComponent> ent, ref ComponentInit args)
        => ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        if (!_timing.IsFirstTimePredicted || args.Handled)
            return;

        args.Handled = true;

        if (ent.Comp.Enabled)
        {
            _audio.Stop(ent.Comp.RunningSound);
            _audio.PlayLocal(ent.Comp.EndSound, ent, ent);
            ent.Comp.DisableAt = _timing.CurTime + ent.Comp.ShiftDelay;
            return;
        }

        ent.Comp.Enabled = true;
        _speed.RefreshMovementSpeedModifiers(ent);

        if (!HasComp<TrailComponent>(ent))
        {
            var trail = AddComp<TrailComponent>(ent);
            trail.RenderedEntity = ent;
            trail.LerpTime = 0.1f;
            trail.LerpDelay = TimeSpan.FromSeconds(4);
            trail.Lifetime = 10;
            trail.Frequency = 0.07f;
            trail.AlphaLerpAmount = 0.2f;
            trail.MaxParticleAmount = 25;
            ent.Comp.Trail = trail;
        }

        if (!HasComp<DogVisionComponent>(ent))
            ent.Comp.DogVision = AddComp<DogVisionComponent>(ent);

        var audio = _audio.PlayLocal(ent.Comp.StartSound, ent, ent);
        if (!audio.HasValue)
            return;

        ent.Comp.RunningSound = audio.Value.Entity;
        Dirty(ent);
    }

    private void OnRefreshSpeed(Entity<SandevistanUserComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Enabled)
            args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private void OnMeleeAttack(Entity<SandevistanUserComponent> ent, ref MeleeAttackEvent args)
    {
        if (!ent.Comp.Enabled
            || !TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon))
            return;

        var rate = weapon.NextAttack - _timing.CurTime; //weapon.AttackRate; breaks things when multiple systems modify NextAttack
        weapon.NextAttack -= rate - rate / ent.Comp.AttackSpeedModifier;
    }

    private void OnMobStateChanged(Entity<SandevistanUserComponent> ent, ref MobStateChangedEvent args)
        => Disable(ent, ent.Comp);

    private void OnShutdown(Entity<SandevistanUserComponent> ent, ref ComponentShutdown args)
    {
        Disable(ent, ent.Comp);
        Del(ent.Comp.ActionUid);
    }

    private void Disable(EntityUid uid, SandevistanUserComponent comp)
    {
        comp.DisableAt = null;
        comp.ColorAccumulator = 0;
        comp.Enabled = false;
        _audio.Stop(comp.RunningSound);
        _speed.RefreshMovementSpeedModifiers(uid);

        if (comp.DogVision != null)
        {
            RemComp<DogVisionComponent>(uid);
            comp.DogVision = null;
        }

        if (comp.Trail != null)
        {
            RemComp<TrailComponent>(uid);
            comp.Trail = null;
        }

        Dirty(uid, comp);
    }
}
