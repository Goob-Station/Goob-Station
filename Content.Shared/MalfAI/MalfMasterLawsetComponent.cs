// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.MalfAI;

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
