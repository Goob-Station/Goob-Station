// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mind.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindLastMobComponent : Component
{
    /// <summary>
    /// The last mob entity this mind was in.
    /// Can be null.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LastMob { get; set; }
}
