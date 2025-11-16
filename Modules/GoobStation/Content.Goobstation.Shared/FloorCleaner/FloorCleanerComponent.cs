// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.FloorCleaner;

/// <summary>
/// This component is for items that can clean stuff like footprints, stains, etcetera.
/// Cleaning != Cleaning forensics.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FloorCleanerComponent : Component
{
    /// <summary>
    /// How long it takes to destroy footprints, strain, etcetera off of things using this entity
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CleanDelay = 8.0f;

    /// <summary>
    /// The X by X box this utensil will clean in.
    /// This is one for bars of soap, three for mops.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Radius = 1;
}
