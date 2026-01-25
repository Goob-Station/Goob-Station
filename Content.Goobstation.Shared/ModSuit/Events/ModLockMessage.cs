using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed class ModLockMessage : BoundUserInterfaceMessage
{
    public NetEntity Module;

    public ModLockMessage(NetEntity module)
    {
        Module = module;
    }
}
