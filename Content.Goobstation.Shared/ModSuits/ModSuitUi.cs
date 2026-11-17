using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public enum ModSuitUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ModSuitActivateMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ModSuitTogglePartMessage(string slot) : BoundUserInterfaceMessage
{
    public readonly string Slot = slot;
}

[Serializable, NetSerializable]
public sealed class ModSuitToggleAllPartsMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ModSuitModuleToggleMessage(NetEntity module) : BoundUserInterfaceMessage
{
    public readonly NetEntity Module = module;
}

[Serializable, NetSerializable]
public sealed class ModSuitModulePinMessage(NetEntity module) : BoundUserInterfaceMessage
{
    public readonly NetEntity Module = module;
}

[Serializable, NetSerializable]
public sealed class ModSuitModuleRemoveMessage(NetEntity module) : BoundUserInterfaceMessage
{
    public readonly NetEntity Module = module;
}
