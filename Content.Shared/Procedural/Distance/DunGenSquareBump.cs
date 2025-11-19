// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Procedural.Distance;

/// <summary>
/// Produces a squarish-shape that's better for filling in most of the area.
/// </summary>
public sealed partial class DunGenSquareBump : IDunGenDistance
{
    [DataField]
    public float BlendWeight { get; set; } = 0.50f;
}
