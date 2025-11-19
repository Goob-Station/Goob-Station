// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Can activate <see cref="LightOnCollideComponent"/> when collided with.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightOnCollideColliderComponent : Component
{
    [DataField]
    public string FixtureId = "lightTrigger";
}
