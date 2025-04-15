// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that ignites surrounding entities when triggered.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteArtifactComponent : Component
{
    [DataField("range")]
    public float Range = 2f;

    [DataField("minFireStack")]
    public int MinFireStack = 2;

    [DataField("maxFireStack")]
    public int MaxFireStack = 5;
}