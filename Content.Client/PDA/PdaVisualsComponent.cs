// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.PDA;

/// <summary>
/// Used for visualizing PDA visuals.
/// </summary>
[RegisterComponent]
public sealed partial class PdaVisualsComponent : Component
{
    public string? BorderColor;

    public string? AccentHColor;

    public string? AccentVColor;
}