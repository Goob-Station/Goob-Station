// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SeeOverLayer;

/// <summary>
/// Marks a mob as able to see through entities that belong to one or more
/// named visual layers (see <see cref="Content.Goobstation.Client.SeeOverLayer.SeeOverLayerVisualsComponent"/>).
///
/// Example: Diona with layers: [kudzu] will see friendly kudzu rendered below
/// their sprite instead of above it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SeeOverLayerComponent : Component
{
    /// <summary>
    /// The set of layer keys this mob can see over.
    /// Must match the <c>Layer</c> field on <see cref="Content.Goobstation.Client.SeeOverLayer.SeeOverLayerVisualsComponent"/>
    /// of the entities you want to see through.
    /// </summary>
    [DataField(required: true)]
    public HashSet<string> Layers = new();
}
