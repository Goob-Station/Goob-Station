// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Borgs;

[Serializable, NetSerializable]
public enum BorgUiKey : byte
{
    Key
}

/// <summary>
/// Send when a player uses the borg BUI to eject a brain.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgEjectBrainBuiMessage : BoundUserInterfaceMessage;

/// <summary>
/// Send when a player uses the borg BUI to eject a power cell.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgEjectBatteryBuiMessage : BoundUserInterfaceMessage;

/// <summary>
/// Send when a player uses the borg BUI to change a borg's name.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgSetNameBuiMessage(string name) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The new name.
    /// </summary>
    public string Name = name;
}

/// <summary>
/// Send when a player uses the borg BUI to remove a borg module.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgRemoveModuleBuiMessage(NetEntity module) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The module to eject.
    /// </summary>
    public NetEntity Module = module;
}
