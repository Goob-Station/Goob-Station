// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Shared.GridPreloader.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Shelter;

[RegisterComponent]
public sealed partial class ShelterCapsuleComponent : Component
{
    [DataField]
    public float DeployTime = 1f;

    [DataField(required: true)]
    public ProtoId<PreloadedGridPrototype> PreloadedGrid;

    [DataField(required: true)]
    public Vector2 BoxSize;

    /// <remarks>
    /// This is needed only to fix the grid. Capsule always should spawn
    /// at the center, and this vector is required to ensure that.
    /// </remarks>>
    [DataField]
    public Vector2 Offset;
}