// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
/// Default metabolism for stimulants and tranqs. Attempts to find a MovementSpeedModifier on the target,
/// adding one if not there and to change the movespeed
/// Trauma - moved it here out of core files and refactored
/// </summary>
public sealed partial class NitriumMovespeedModifier : EntityEffectBase<NitriumMovespeedModifier>
{
    /// <summary>
    /// How much the entities' walk speed is multiplied by.
    /// </summary>
    [DataField]
    public float SpeedModifier = 1f;

    /// <summary>
    /// How long the modifier refreshes for
    /// </summary>
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(6f);

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-movespeed-modifier",
            ("chance", Probability),
            ("walkspeed", SpeedModifier),
            ("sprintspeed", SpeedModifier),
            ("time", Time.TotalSeconds));
}

/// <summary>
/// Remove reagent at set rate, changes the movespeed modifiers and adds a MovespeedModifierMetabolismComponent if not already there.
/// </summary>
public sealed class NitriumMovespeedModifierEffectSystem : EntityEffectSystem<InputMoverComponent, NitriumMovespeedModifier>
{
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;

    public static readonly EntProtoId StatusEffect = "NitriumStatusEffect";

    protected override void Effect(Entity<InputMoverComponent> ent, ref EntityEffectEvent<NitriumMovespeedModifier> args)
    {
        _movementMod.TryAddMovementSpeedModDuration(ent, StatusEffect, args.Effect.Time, args.Effect.SpeedModifier);
    }
}
