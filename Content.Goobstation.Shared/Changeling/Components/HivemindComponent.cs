// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Used for identifying other changelings.
///     Indicates that a changeling has bought the hivemind access ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HivemindComponent : Component
{
}