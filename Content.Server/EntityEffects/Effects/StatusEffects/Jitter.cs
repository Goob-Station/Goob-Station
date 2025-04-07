// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Jittering;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.StatusEffects;

/// <summary>
///     Adds the jitter status effect to a mob.
///     This doesn't use generic status effects because it needs to
///     take in some parameters that JitterSystem needs.
/// </summary>
public sealed partial class Jitter : EntityEffect
{
    [DataField]
    public float Amplitude = 10.0f;

    [DataField]
    public float Frequency = 4.0f;

    [DataField]
    public float Time = 2.0f;

    /// <remarks>
    ///     true - refresh jitter time,  false - accumulate jitter time
    /// </remarks>
    [DataField]
    public bool Refresh = true;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var time = Time;
        if (args is EntityEffectReagentArgs reagentArgs)
            time *= reagentArgs.Scale.Float();

        args.EntityManager.EntitySysManager.GetEntitySystem<SharedJitteringSystem>()
            .DoJitter(args.TargetEntity, TimeSpan.FromSeconds(time), Refresh, Amplitude, Frequency);
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-jittering", ("chance", Probability));
}