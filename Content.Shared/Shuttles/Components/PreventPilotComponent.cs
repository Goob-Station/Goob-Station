// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

/// <summary>
/// Add to grids that you do not want manually piloted under any circumstances.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventPilotComponent : Component
{

}
