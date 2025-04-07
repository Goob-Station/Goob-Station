// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared.PowerCell.Components;

/// <summary>
/// Integrate PowerCellDraw and ItemToggle.
/// Make toggling this item require power, and deactivates the item when power runs out.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleCellDrawComponent : Component;