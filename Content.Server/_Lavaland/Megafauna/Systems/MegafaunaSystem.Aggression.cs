// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Aggression;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    private void InitializeAggression()
    {
        SubscribeLocalEvent<AggressiveMegafaunaAiComponent, AttackedEvent>(OnAttacked);
    }

    private void UpdateAggression(float frameTime)
    {
        var query = EntityQueryEnumerator<AggressiveMegafaunaAiComponent, MegafaunaAiComponent, AggressiveComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var aggroAi, out var ai, out var aggressive, out var damageable))
        {
            aggroAi.UpdateAccumulator -= frameTime;
            if (aggroAi.UpdateAccumulator > 0f)
                continue;
            aggroAi.UpdateAccumulator = aggroAi.UpdatePeriod;

            if (!ai.Active)
                continue;

            var angerMultiplier = 1f;
            var healthMultiplier = 1f;
            if (aggressive.Aggressors.Count > 1)
            {
                angerMultiplier = (aggressive.Aggressors.Count - 1) * aggroAi.AngerScalingFactor;
                healthMultiplier = (aggressive.Aggressors.Count - 1) * aggroAi.HealthScalingFactor;
            }

            var maxUnscaledHp = aggroAi.HpAgressionLimit ?? ai.BaseTotalHp;
            var newMinAnger = Math.Max((float) (damageable.TotalDamage / (maxUnscaledHp * healthMultiplier)) * aggroAi.MaxAnger - 1f, 0f) + 1f;
            aggroAi.MinAnger = newMinAnger * angerMultiplier;
            UpdateAggression((uid, aggroAi));
        }
    }

    private void OnAttacked(Entity<AggressiveMegafaunaAiComponent> ent, ref AttackedEvent args)
        => AdjustAggression(ent, ent.Comp.AdjustAngerOnAttack);

    public void AdjustAggression(Entity<AggressiveMegafaunaAiComponent> ent, float value)
    {
        ent.Comp.CurrentAnger += value;
        UpdateAggression(ent);
    }

    private void UpdateAggression(Entity<AggressiveMegafaunaAiComponent> ent)
        => ent.Comp.CurrentAnger = Math.Clamp(ent.Comp.CurrentAnger, ent.Comp.MinAnger, ent.Comp.MaxAngerHardCap);

    private void UpdateScaledThresholds(Entity<MegafaunaAiComponent, AggressiveComponent, AggressiveMegafaunaAiComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp3))
            return;

        var playerCount = Math.Max(1, ent.Comp2.Aggressors.Count);
        var scalingMultiplier = 1f;

        for (var i = 1; i < playerCount; i++)
            scalingMultiplier *= ent.Comp3.HealthScalingFactor;

        if (_threshold.TryGetDeadThreshold(ent, out var deadThreshold)
            && deadThreshold < ent.Comp1.BaseTotalHp * scalingMultiplier)
            _threshold.SetMobStateThreshold(ent, ent.Comp1.BaseTotalHp * scalingMultiplier, MobState.Dead);
    }
}
