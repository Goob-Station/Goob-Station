// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Flickers all the lights within a certain radius.
/// </summary>
[RegisterComponent]
public sealed partial class LightFlickerArtifactComponent : Component
{
    /// <summary>
    /// Lights within this radius will be flickered on activation
    /// </summary>
    [DataField("radius")]
    public float Radius = 4;

    /// <summary>
    /// The chance that the light will flicker
    /// </summary>
    [DataField("flickerChance")]
    public float FlickerChance = 0.75f;
}