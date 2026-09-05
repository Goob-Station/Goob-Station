// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

/// <summary>
/// Any chicken with this component can be plated with ores
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlateableChickenComponent : Component;
