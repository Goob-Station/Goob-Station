// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Wires;

/// <summary>
/// This is used for activatable UIs that require the entity to have a panel in a certain state.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedWiresSystem))]
public sealed partial class ActivatableUIRequiresPanelComponent : Component
{
    /// <summary>
    /// TRUE: the panel must be open to access the UI.
    /// FALSE: the panel must be closed to access the UI.
    /// </summary>
    [DataField]
    public bool RequireOpen = true;
}