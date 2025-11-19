// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.PowerCell.Components;

/// <summary>
/// Integrate PowerCellDraw and ItemToggle.
/// Make toggling this item require power, and deactivates the item when power runs out.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleCellDrawComponent : Component;
