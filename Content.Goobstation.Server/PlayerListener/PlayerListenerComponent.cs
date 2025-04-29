// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;

namespace Content.Goobstation.Server.PlayerListener;

/// <summary>
///     Stores data about players, listens even.
/// </summary>
[RegisterComponent]
public sealed partial class PlayerListenerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public readonly HashSet<NetUserId> UserIds = [];
}