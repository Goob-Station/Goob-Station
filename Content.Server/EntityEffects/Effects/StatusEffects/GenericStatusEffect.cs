// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.StatusEffects;

/// <summary>
///     Adds a generic status effect to the entity,
///     not worrying about things like how to affect the time it lasts for
///     or component fields or anything. Just adds a component to an entity
///     for a given time. Easy.
/// </summary>
/// <remarks>
///     Can be used for things like adding accents or something. I don't know. Go wild.
/// </remarks>
[UsedImplicitly]
public sealed partial class GenericStatusEffect : EntityEffect
{
    [DataField(required: true)]
    public string Key = default!;

    [DataField]
    public string Component = String.Empty;

    [DataField]
    public float Time = 2.0f;

    /// <remarks>
    ///     true - refresh status effect time,  false - accumulate status effect time
    /// </remarks>
    [DataField]
    public bool Refresh = true;

    /// <summary>
    ///     Should this effect add the status effect, remove time from it, or set its cooldown?
    /// </summary>
    [DataField]
    public StatusEffectMetabolismType Type = StatusEffectMetabolismType.Add;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var statusSys = args.EntityManager.EntitySysManager.GetEntitySystem<StatusEffectsSystem>();

        var time = Time;
        if (args is EntityEffectReagentArgs reagentArgs)
            time *= reagentArgs.Scale.Float();

        if (Type == StatusEffectMetabolismType.Add && Component != String.Empty)
        {
            statusSys.TryAddStatusEffect(args.TargetEntity, Key, TimeSpan.FromSeconds(time), Refresh, Component);
        }
        else if (Type == StatusEffectMetabolismType.Remove)
        {
            statusSys.TryRemoveTime(args.TargetEntity, Key, TimeSpan.FromSeconds(time));
        }
        else if (Type == StatusEffectMetabolismType.Set)
        {
            statusSys.TrySetTime(args.TargetEntity, Key, TimeSpan.FromSeconds(time));
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString(
        "reagent-effect-guidebook-status-effect",
        ("chance", Probability),
        ("type", Type),
        ("refresh", Refresh),
        ("time", Time),
        ("key", $"reagent-effect-status-effect-{Key}"));
}

public enum StatusEffectMetabolismType
{
    Add,
    Remove,
    Set
}