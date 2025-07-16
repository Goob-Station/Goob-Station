// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;

// ReSharper disable EnforceForStatementBraces
namespace Content.Shared._Lavaland.Anger;

public sealed class SharedAngerSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _threshold = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AngerComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<AngerComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<AngerComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<AngerComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
        SubscribeLocalEvent<AngerComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<AngerComponent, AggressorRemovedEvent>(OnAggressorRemoved);
    }

    public void AdjustAggression(Entity<AngerComponent> ent, float value)
    {
        ent.Comp.CurrentAnger += value;
        UpdateAggression(ent.Owner);
    }

    public void UpdateAggression(Entity<AngerComponent?, AggressiveComponent?, DamageableComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3))
            return;

        var anger = ent.Comp1;
        var aggressive = ent.Comp2;
        var damage = ent.Comp3;

        var angerMultiplier = 1f;
        var healthMultiplier = 1f;
        if (aggressive.Aggressors.Count > 1)
        {
            angerMultiplier = (aggressive.Aggressors.Count - 1) * anger.AngerScalingFactor;
            healthMultiplier = (aggressive.Aggressors.Count - 1) * anger.HealthScalingFactor;
        }

        var maxUnscaledHp = anger.HpAgressionLimit ?? anger.BaseTotalHp;
        var newMinAnger = Math.Max((float) (damage.TotalDamage / (maxUnscaledHp * healthMultiplier)) * anger.MaxAnger - 1f, 0f) + 1f;
        anger.MinAnger = newMinAnger * angerMultiplier;
        anger.CurrentAnger = Math.Clamp(anger.CurrentAnger, anger.MinAnger, anger.MaxAngerHardCap);
    }

    public void UpdateScaledThresholds(Entity<AngerComponent?, AggressiveComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2))
            return;

        var playerCount = Math.Max(1, ent.Comp2.Aggressors.Count);
        var scalingMultiplier = 1f;

        for (var i = 1; i < playerCount; i++)
            scalingMultiplier *= ent.Comp1.HealthScalingFactor;

        if (_threshold.TryGetDeadThreshold(ent, out var deadThreshold)
            && deadThreshold < ent.Comp1.BaseTotalHp * scalingMultiplier)
            _threshold.SetMobStateThreshold(ent, ent.Comp1.BaseTotalHp * scalingMultiplier, MobState.Dead);
    }

    #region Event Handling

    private void OnAggressorAdded(Entity<AngerComponent> ent, ref AggressorAddedEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnAggressorRemoved(Entity<AngerComponent> ent, ref AggressorRemovedEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnAttacked(Entity<AngerComponent> ent, ref AttackedEvent args)
        => AdjustAggression(ent, ent.Comp.AdjustAngerOnAttack);

    private void OnDamaged(Entity<AngerComponent> ent, ref DamageChangedEvent args)
        => UpdateAggression(ent.Owner);

    private void OnMegafaunaStartup(Entity<AngerComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (!_threshold.TryGetDeadThreshold(ent.Owner, out var threshold))
        {
            Log.Error($"Megafauna {ToPrettyString(ent)} didn't have MobThresholdComponent when trying to startup a boss!");
            return;
        }

        ent.Comp.BaseTotalHp = threshold.Value;
    }

    private void OnMegafaunaShutdown(Entity<AngerComponent> ent, ref MegafaunaShutdownEvent args)
        => UpdateScaledThresholds(ent.Owner);

    #endregion
}
