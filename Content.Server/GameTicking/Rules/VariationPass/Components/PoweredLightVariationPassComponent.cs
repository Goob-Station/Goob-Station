// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.VariationPass.Components;

/// <summary>
/// This handle randomly destroying lights, causing them to flicker endlessly, or replacing their tube/bulb with different variants.
/// </summary>
[RegisterComponent]
public sealed partial class PoweredLightVariationPassComponent : Component
{
    /// <summary>
    ///     Chance that a light will be replaced with a broken variant.
    /// </summary>
    [DataField]
    public float LightBreakChance = 0.15f;

    /// <summary>
    ///     Chance that a light will be replaced with an aged variant.
    /// </summary>
    [DataField]
    public float LightAgingChance = 0.05f;

    [DataField]
    public float AgedLightTubeFlickerChance = 0.03f;

    [DataField]
    public EntProtoId BrokenLightBulbPrototype = "LightBulbBroken";

    [DataField]
    public EntProtoId BrokenLightTubePrototype = "LightTubeBroken";

    [DataField]
    public EntProtoId AgedLightBulbPrototype = "LightBulbOld";

    [DataField]
    public EntProtoId AgedLightTubePrototype = "LightTubeOld";
}