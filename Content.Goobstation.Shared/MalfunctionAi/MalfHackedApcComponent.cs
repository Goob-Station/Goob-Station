// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Marks an APC that has already been hacked by a Malfunction AI, so it cannot be
/// hacked again for more processing power.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfHackedApcComponent : Component;
