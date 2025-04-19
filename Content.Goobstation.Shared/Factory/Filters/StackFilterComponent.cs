// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// A filter that requires items to have a minimum stack size.
/// Non-stackable items will always be blocked.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AutomationFilterSystem))]
[AutoGenerateComponentState]
public sealed partial class StackFilterComponent : Component
{
    /// <summary>
    /// Minimum stack size to require.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Size = 1;
}

[Serializable, NetSerializable]
public enum StackFilterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class StackFilterSetSizeMessage(int size) : BoundUserInterfaceMessage
{
    public readonly int Size = size;
}
