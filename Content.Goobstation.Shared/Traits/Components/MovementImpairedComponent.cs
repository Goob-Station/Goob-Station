// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Traits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MovementImpairedComponent : Component
{
    /// <summary>
    /// What number is this entities speed multiplied by when impaired?
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ImpairedSpeedMultiplier = 0.60;

    /// <summary>
    /// The original speed multiplier of the entity, stored and restored when the item is picked up or put down.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 BaseImpairedSpeedMultiplier = 0.60;

    /// <summary>
    /// Which items are overflowing the cap, and by how much.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<EntityUid, FixedPoint2> SpeedCorrectionOverflow = new();

    /// <summary>
    /// How many fully movement correcting items the entity has.
    /// </summary>
    /// <remarks>
    /// This means how many items with a correction value of "0" the entity has.
    /// This prevents a lot of fuckery.
    /// </remarks>
    [DataField]
    public int CorrectionCounter;
}
