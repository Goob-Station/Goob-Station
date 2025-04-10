// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.RandomizeMovementSpeed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// The minimum limit of the modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Min = 0.6f;

    /// <summary>
    /// The max limit of the modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Max = 1.6f;

    /// <summary>
    /// The current modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentModifier = 1f;

    /// <summary>
    /// Next execution time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextExecutionTime;

    /// <summary>
    /// The Uid of the entity that picked up the item.
    /// </summary>
    [DataField]
    public EntityUid EntityUid;

    /// <summary>
    /// What to restrict the item to
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

}
