// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.GameStates;
using Content.Shared.Toggleable;

namespace Content.Shared.Light.Components;

/// <summary>
/// Makes <see cref="ItemToggledEvent"/> enable and disable point lights on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemTogglePointLightComponent : Component
{
    /// <summary>
    /// When true, causes the color specified in <see cref="ToggleableVisuals.Color"/>
    /// be used to modulate the color of lights on this entity.
    /// </summary>
    [DataField] public bool ToggleableVisualsColorModulatesLights = false;
}
