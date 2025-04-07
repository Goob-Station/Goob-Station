// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
/// Triggers when an instrument is played nearby
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactMusicTriggerComponent : Component
{
    /// <summary>
    /// how close does the artifact have to be to the instrument to activate
    /// </summary>
    [DataField("range")]
    public float Range = 5;
}