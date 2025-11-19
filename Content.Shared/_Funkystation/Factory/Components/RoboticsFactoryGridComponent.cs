// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Factory.Components;

/// <summary>
/// Marker component for a robotics factory grid (prototype references use this name).
/// Placed in Shared so both client and server know about it during prototype load.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RoboticsFactoryGridComponent : Component
{
}
