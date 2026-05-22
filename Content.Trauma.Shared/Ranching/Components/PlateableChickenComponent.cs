// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Any chicken with this component can be plated with ores
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlateableChickenComponent : Component;
