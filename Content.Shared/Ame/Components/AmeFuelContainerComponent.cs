// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Ame.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AmeFuelContainerComponent : Component
{
    /// <summary>
    /// The amount of fuel in the container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FuelAmount = 500;

    /// <summary>
    /// The maximum fuel capacity of the container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FuelCapacity = 500;
}
