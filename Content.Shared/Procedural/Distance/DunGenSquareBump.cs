// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Procedural.Distance;

/// <summary>
/// Produces a squarish-shape that's better for filling in most of the area.
/// </summary>
public sealed partial class DunGenSquareBump : IDunGenDistance
{
    [DataField]
    public float BlendWeight { get; set; } = 0.50f;
}