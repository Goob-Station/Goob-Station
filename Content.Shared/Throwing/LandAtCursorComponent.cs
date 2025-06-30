// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Throwing
{
    /// <summary>
    ///     Makes an item land at the cursor when thrown and slide a little further.
    ///     Without it the item lands slightly in front and stops moving at the cursor.
    ///     Use this for throwing weapons that should pierce the opponent, for example spears.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class LandAtCursorComponent : Component { }
}