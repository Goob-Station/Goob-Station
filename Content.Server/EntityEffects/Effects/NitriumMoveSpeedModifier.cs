// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
/// Default metabolism for stimulants and tranqs. Attempts to find a MovementSpeedModifier on the target,
/// adding one if not there and to change the movespeed
/// </summary>
public sealed partial class NitriumMovespeedModifier : EntityEffect
{
    /// <summary>
    /// How much the entities' walk speed is multiplied by.
    /// </summary>
    [DataField]
    public float WalkSpeedModifier { get; set; } = 1;

    /// <summary>
    /// How much the entities' run speed is multiplied by.
    /// </summary>
    [DataField]
    public float SprintSpeedModifier { get; set; } = 1;

    /// <summary>
    /// How long the modifier applies (in seconds).
    /// Is scaled by reagent amount if used with an EntityEffectReagentArgs.
    /// </summary>
    [DataField]
    public float StatusLifetime = 6f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-movespeed-modifier",
            ("chance", Probability),
            ("walkspeed", WalkSpeedModifier),
            ("sprintspeed", SprintSpeedModifier),
            ("time", StatusLifetime));
    }

    /// <summary>
    /// Remove reagent at set rate, changes the movespeed modifiers and adds a MovespeedModifierMetabolismComponent if not already there.
    /// </summary>
    public override void Effect(EntityEffectBaseArgs args)
    {
        var status = args.EntityManager.EnsureComponent<MovespeedModifierMetabolismComponent>(args.TargetEntity);

        // Only refresh movement if we need to.
        var modified = !status.WalkSpeedModifier.Equals(WalkSpeedModifier) ||
                       !status.SprintSpeedModifier.Equals(SprintSpeedModifier);

        status.WalkSpeedModifier = WalkSpeedModifier;
        status.SprintSpeedModifier = SprintSpeedModifier;

        SetTimer(status, StatusLifetime);

        if (modified)
            args.EntityManager.System<MovementSpeedModifierSystem>().RefreshMovementSpeedModifiers(args.TargetEntity);
    }
    public void SetTimer(MovespeedModifierMetabolismComponent status, float time)
    {
        var gameTiming = IoCManager.Resolve<IGameTiming>();

        status.ModifierTimer = TimeSpan.FromSeconds(gameTiming.CurTime.TotalSeconds + time);
        status.Dirty();
    }
}