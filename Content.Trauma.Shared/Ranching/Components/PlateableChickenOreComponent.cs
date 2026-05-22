// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Attached to ores that can be used with <see cref="PlateableChickenComponent"/>>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlateableChickenOreComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components;
}
