// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
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
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sandevistan;

public sealed class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
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

        var query = EntityQueryEnumerator<SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.DisableAt != null
                && _timing.CurTime > comp.DisableAt)
            {
                Disable(uid, comp);
                comp.DisableAt = null;
            }

            if (comp.Trail != null)
            {
                comp.Trail.Color = Color.FromHsv(new Vector4(comp.ColorAccumulator % 100f / 100f, 1, 1, 1));
                Dirty(uid, comp.Trail);
                comp.ColorAccumulator++;
            }

            if (comp.NextExecutionTime > _timing.CurTime)
                return;

            var multiplier = frameTime * _timing.TickRate;
            comp.NextExecutionTime = _timing.CurTime + comp.UpdateDelay;

            if (!comp.Enabled)
            {
                comp.CurrentLoad = MathF.Max(0, comp.CurrentLoad + comp.LoadPerInactiveSecond * multiplier);
                return;
            }

            comp.CurrentLoad += comp.LoadPerActiveSecond * multiplier;

            var stateActions = new Dictionary<int, Action>
            {
                { 2, () => _jittering.DoJitter(uid, comp.StatusEffectTime, true)},
                { 3, () => _stamina.TakeStaminaDamage(uid, comp.StaminaDamage * multiplier)},
                { 4, () => _damageable.TryChangeDamage(uid, comp.Damage * multiplier)},
                { 5, () => _stun.TryKnockdown(uid, comp.StatusEffectTime, true)},
                { 6, () => Disable(uid, comp)},
                { 7, () => _mobState.ChangeMobState(uid, MobState.Dead)},
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
                return;

            var popup = 0;
            foreach (var state in filteredStates)
            {
                if (state is < 1 or > 4)
                    continue;
                if (state > popup)
                    popup = state;
            }

            if (popup == 0)
                return;

            _popup.PopupEntity(Loc.GetString("sandevistan-overload-" + popup), uid, uid);
            comp.NextPopupTime = _timing.CurTime + comp.PopupDelay;
        }
    }

    private void OnInit(Entity<SandevistanUserComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);
    }

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        args.Handled = true;

        if (ent.Comp.Enabled)
        {
            ent.Comp.ColorAccumulator = 0;
            _audio.Stop(ent.Comp.RunningSound);
            _audio.PlayEntity(ent.Comp.EndSound, ent, ent);
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
            Dirty(ent, trail);
            ent.Comp.Trail = trail;
        }

        if (!HasComp<DogVisionComponent>(ent))
            ent.Comp.DogVision = AddComp<DogVisionComponent>(ent);

        if (_net.IsClient) // Fuck sound networking
            return;

        var audio = _audio.PlayEntity(ent.Comp.StartSound, ent, ent);
        if (!audio.HasValue)
            return;

        ent.Comp.RunningSound = audio.Value.Entity;
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
    {
        Disable(ent, ent.Comp);
    }

    private void OnShutdown(Entity<SandevistanUserComponent> ent, ref ComponentShutdown args)
    {
        Disable(ent, ent.Comp);
        Del(ent.Comp.ActionUid);
    }

    private void Disable(EntityUid uid, SandevistanUserComponent comp)
    {
        comp.ColorAccumulator = 0;
        comp.Enabled = false;
        _audio.Stop(comp.RunningSound);
        _speed.RefreshMovementSpeedModifiers(uid);

        if (comp.DogVision != null)
        {
            RemComp(uid, comp.DogVision);
            comp.DogVision = null;
        }

        if (comp.Trail != null)
        {
            RemComp(uid, comp.Trail);
            comp.Trail = null;
        }
    }
}
