// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Traits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MovementImpairedComponent : Component
{
    /// <summary>
    /// What number is this entities speed multiplied by when impaired?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ImpairedSpeedMultiplier { get; set; } = 0.6f;

    /// <summary>
    /// The original speed multiplier of the entity, stored and restored when the item is picked up or put down.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BaseImpairedSpeedMultiplier { get; set; } = 0.6f;

    /// <summary>
    /// How many movement correcting items the entity has.
    /// </summary>
    [DataField]
    public int CorrectionCounter { get; set; } = 0;
}
