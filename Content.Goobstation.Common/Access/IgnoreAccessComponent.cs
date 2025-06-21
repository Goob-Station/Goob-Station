// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Access;

/// <summary>
/// If assigned to an entity with AccessReaderComponent, will always allow access for some ignored entities.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IgnoreAccessComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> Ignore = new();
}
