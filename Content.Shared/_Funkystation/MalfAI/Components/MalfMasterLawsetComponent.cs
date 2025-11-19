// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI.Components;

/// <summary>
/// Attached to the Malf AI rule entity to store a master lawset for later use.
/// Server-only logic will consume this; no visuals/UI here.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfMasterLawsetComponent : Component
{
    /// <summary>
    /// The canonical master lawset lines. Empty until populated by systems later.
    /// </summary>
    [DataField] public List<string> Laws = new();
}
