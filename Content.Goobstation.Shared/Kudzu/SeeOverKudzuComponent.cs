// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Kudzu;

/// <summary>
/// Marks mobs that should render friendly kudzu beneath their sprite for the local viewer.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SeeOverKudzuComponent : Component
{
}
