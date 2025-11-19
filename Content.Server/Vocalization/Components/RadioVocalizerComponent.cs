// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Vocalization.Components;

/// <summary>
/// Makes an entity able to vocalize through an equipped radio
/// </summary>
[RegisterComponent]
public sealed partial class RadioVocalizerComponent : Component
{
    /// <summary>
    /// chance the vocalizing entity speaks on the radio.
    /// </summary>
    [DataField]
    public float RadioAttemptChance = 0.6f;
}
