// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Aggression;
using Content.Server._Lavaland.Megafauna;
using Content.Server._Lavaland.Megafauna.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Hierophant.Actions;

public sealed partial class HierophantChasersAction : BaseHierophantAction
{
    [DataField]
    public EntProtoId ChaserTile = "LavalandHierophantChaser";

    [DataField]
    public float AfterDelay = 1.5f;

    [DataField]
    public float BaseSpeed = 3f;

    [DataField]
    public float MinSpeed = 1f;

    [DataField]
    public float MaxSpeed = 4f;

    [DataField]
    public int BaseAdditionalSteps = 10;

    [DataField]
    public int MinSteps = 20;

    [DataField]
    public int MaxSteps = 50;

    [DataField]
    public int MinAmount = 1;

    [DataField("speedMultiplier")]
    public float SpeedAggressionMultiplier = 2.5f;

    [DataField("stepsMultiplier")]
    public float StepsAggressionMultiplier = 4f;

    [DataField("amountMultiplier")]
    public float AmountAggressionMultiplier = 0.6f;

    public override float Invoke(MegafaunaAttackBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var aggroSystem = entMan.System<AggressorsSystem>();
        var hieroSystem = entMan.System<HierophantSystem>();

        var anger = entMan.GetComponentOrNull<AggressiveMegafaunaAiComponent>(uid)?.CurrentAnger;
        aggroSystem.TryPickTarget(uid, out var target);
        target ??= uid;

        var speed = Math.Min(anger != null ? Math.Max(anger.Value * SpeedAggressionMultiplier, MinSpeed) : BaseSpeed, MaxSpeed);
        var steps = Math.Min((anger != null ? (int) MathF.Round(anger.Value * StepsAggressionMultiplier) : BaseAdditionalSteps) + MinSteps, MaxSteps);
        var amount = anger != null ? Math.Max((int) MathF.Round(anger.Value * AmountAggressionMultiplier), MinAmount) : MinAmount;
        hieroSystem.SpawnChasers(uid, DamageTile, speed, steps, target, amount);

        return AfterDelay;
    }
}
