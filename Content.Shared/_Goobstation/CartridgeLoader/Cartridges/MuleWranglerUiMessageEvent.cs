using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MuleWranglerUiMessageEvent(MuleWranglerMessageType type, NetEntity muleEntity, NetEntity? dropOffEntity) : CartridgeMessageEvent
{
    public readonly MuleWranglerMessageType Type = type;

    public readonly NetEntity MuleEntity = muleEntity;

    public readonly NetEntity? DropOffEntity = dropOffEntity;
}

public enum MuleWranglerMessageType
{
    Return,
    Transport,
    SetDestination,
    Unload,
}
