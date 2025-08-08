﻿using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Hierophant.Components;

/// <summary>
/// Marker component that makes this entity immune to Hierophant's damage tiles.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HierophantImmuneComponent : Component
{
    /// <summary>
    /// Time when this immunity will end and component will remove itself.
    /// </summary>
    [DataField]
    public TimeSpan? EndTime;
}
