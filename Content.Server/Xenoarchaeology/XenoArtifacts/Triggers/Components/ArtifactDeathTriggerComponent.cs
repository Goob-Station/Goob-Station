// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
/// Triggers when a nearby entity dies
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactDeathTriggerComponent : Component
{
    /// <summary>
    /// How close to the death the artifact has to be for it to trigger.
    /// </summary>
    [DataField("range")]
    public float Range = 15f;
}