// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Anger.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;

// ReSharper disable EnforceForStatementBraces
namespace Content.Shared._Lavaland.Anger.Systems;

public sealed class AngerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly MobPhasesSystem _phases = default!;

    private EntityQuery<AngerPlayerScalingComponent> _scalingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AngerComponent, MapInitEvent>(OnCompStartup);
        SubscribeLocalEvent<AngerComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<AngerComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<AngerComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
        SubscribeLocalEvent<AngerComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<AngerComponent, AggressorRemovedEvent>(OnAggressorRemoved);

        SubscribeLocalEvent<AdjustAngerOnHitComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<AngerDelayActionComponent, ActionPerformedEvent>(OnAngerActionUsed);

        _scalingQuery = GetEntityQuery<AngerPlayerScalingComponent>();
    }

    public void AdjustAggression(Entity<AngerComponent?> ent, float value)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

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

        // Get multipliers if possible
        var angerMultiplier = 1f;
        var healthMultiplier = 1f;
        if (_scalingQuery.TryComp(ent.Owner, out var scaling)
            && aggressive.Aggressors.Count > 1)
        {
            var playerCount = Math.Max(1, ent.Comp2.Aggressors.Count);
            if (scaling.AngerScalingFactor != null)
            {
                for (var i = 1; i < playerCount; i++)
                    angerMultiplier *= scaling.AngerScalingFactor.Value;
            }
            if (scaling.HealthScalingFactor != null)
            {
                for (var i = 1; i < playerCount; i++)
                    healthMultiplier *= scaling.HealthScalingFactor.Value;
            }
        }

        // Save how much anger was added by external sources
        var addedAnger = anger.CurrentAnger - anger.MinAnger;

        // Progress between 0 and 1 until reaching death
        var healthProgress = Math.Max((float) (damage.TotalDamage / (anger.TotalHp * healthMultiplier)), 0f);

        // Amount of anger based on progress between Min and Max angers
        var newMinAnger = anger.DefaultMinAnger + (anger.DefaultMaxAnger - anger.DefaultMinAnger) * healthProgress;

        // Set Minimum and Maximum anger accounting for anger multiplier
        // Order is important so MinAnger won't overflow!
        anger.MaxAnger = Math.Min(anger.DefaultMaxAnger * angerMultiplier, anger.AngerHardcap);
        anger.MinAnger = Math.Min(newMinAnger * angerMultiplier, anger.MaxAnger);

        // Set current anger and account for hard cap just to be safe.
        anger.CurrentAnger =
            Math.Min(Math.Clamp(anger.CurrentAnger, anger.MinAnger, anger.MaxAnger) + addedAnger, anger.AngerHardcap);
    }

    public void UpdateScaledThresholds(Entity<AngerComponent?, AggressiveComponent?, AngerPlayerScalingComponent?> ent)
    {
        UpdateAggression((ent.Owner, ent.Comp1, ent.Comp2));

        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3, false)
            || ent.Comp3.HealthScalingFactor == null)
            return;

        var playerCount = Math.Max(1, ent.Comp2.Aggressors.Count);
        var scalingMultiplier = 1f;

        for (var i = 1; i < playerCount; i++)
            scalingMultiplier *= ent.Comp3.HealthScalingFactor.Value;

        _threshold.SetMobStateThreshold(ent.Owner, ent.Comp1.TotalHp * scalingMultiplier, MobState.Dead);

        _phases.UnscaleAllPhaseThresholds(ent.Owner);
        _phases.ScaleAllPhaseThresholds(ent.Owner, scalingMultiplier);
    }

    /// <summary>
    /// Returns the scaled number that is between min and max based on current anger level.
    /// </summary>
    public float GetAngerScale(Entity<AngerComponent?> ent, float min = 0f, float max = 1f, bool inverse = false)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return min; // Minimal possible value as if anger was 0

        var maxAnger = ent.Comp.MaxAnger;
        var anger = ent.Comp.CurrentAnger;
        var progress = anger / maxAnger;
        return inverse
            ? max + (min - max) * (1f - progress)
            : min + (max - min) * progress;
    }

    /// <summary>
    /// Returns the scaled number that is between min and max based on current anger level.
    /// </summary>
    public int GetAngerScale(Entity<AngerComponent?> ent, int min = 0, int max = 10, bool inverse = false)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return min; // Minimal possible value as if anger was 0

        var maxAnger = ent.Comp.MaxAnger;
        var anger = Math.Max(ent.Comp.CurrentAnger - ent.Comp.DefaultMinAnger, 0f);
        var progress = anger / maxAnger;
        return inverse
            ? (int) Math.Round(max + (min - max) * (1f - progress))
            : (int) Math.Round(min + (max - min) * progress);
    }

    /// <summary>
    /// Returns the scaled time that is between min and max based on current anger level.
    /// </summary>
    public TimeSpan GetAngerScale(Entity<AngerComponent?> ent, TimeSpan min, TimeSpan max, bool inverse = false)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return min; // Minimal possible value as if anger was 0

        var maxAnger = Math.Max(ent.Comp.MaxAnger, 0.1f);
        var anger = ent.Comp.CurrentAnger;
        var progress = anger / maxAnger;
        return inverse
            ? max + (min - max) * progress
            : min + (max - min) * (1f - progress);
    }

    #region Event Handling

    private void OnCompStartup(Entity<AngerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.MaxAnger = ent.Comp.DefaultMaxAnger;
        ent.Comp.MinAnger = ent.Comp.DefaultMinAnger;
    }

    private void OnAggressorAdded(Entity<AngerComponent> ent, ref AggressorAddedEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnAggressorRemoved(Entity<AngerComponent> ent, ref AggressorRemovedEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnDamaged(Entity<AngerComponent> ent, ref DamageChangedEvent args)
        => UpdateAggression(ent.Owner);

    private void OnMegafaunaStartup(Entity<AngerComponent> ent, ref MegafaunaStartupEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnMegafaunaShutdown(Entity<AngerComponent> ent, ref MegafaunaShutdownEvent args)
        => UpdateScaledThresholds(ent.Owner);

    private void OnAttacked(Entity<AdjustAngerOnHitComponent> ent, ref AttackedEvent args)
        => AdjustAggression(ent.Owner, ent.Comp.AdjustAngerOnAttack);

    private void OnAngerActionUsed(Entity<AngerDelayActionComponent> ent, ref ActionPerformedEvent args)
    {
        var comp = ent.Comp;
        var delay = GetAngerScale(args.Performer, comp.MinDelay, comp.MaxDelay, comp.Inverse);

        _actions.SetUseDelay(ent.Owner, delay);
        _actions.StartUseDelay(ent.Owner);
    }

    #endregion
}
