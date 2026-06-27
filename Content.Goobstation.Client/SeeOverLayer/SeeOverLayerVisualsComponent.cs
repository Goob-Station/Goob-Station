// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.SeeOverLayer;

/// <summary>
/// Allows an entity's draw depth to be overridden for local viewers that have
/// <see cref="Content.Goobstation.Shared.SeeOverLayer.SeeOverLayerComponent"/> with a matching layer key.
///
/// Add this to any entity you want to appear below certain mobs' sprites.
/// Configure <c>NormalDrawDepth</c> and <c>SeeOverDrawDepth</c> in YAML to
/// control the visual result.
/// </summary>
[RegisterComponent]
public sealed partial class SeeOverLayerVisualsComponent : Component
{
    /// <summary>
    /// The layer key that viewer mobs must include in their
    /// <see cref="Content.Goobstation.Shared.SeeOverLayer.SeeOverLayerComponent.Layers"/>
    /// to trigger the depth override.
    /// </summary>
    [DataField(required: true)]
    public string Layer = string.Empty;

    /// <summary>Draw depth used when the local player cannot see over this entity.</summary>
    [DataField]
    public int NormalDrawDepth = (int) Content.Shared.DrawDepth.DrawDepth.Overdoors;

    /// <summary>Draw depth used when the local player can see over this entity.</summary>
    [DataField]
    public int SeeOverDrawDepth = (int) Content.Shared.DrawDepth.DrawDepth.HighFloorObjects;
}
