using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed class ModModuleDeactivateMessage : BoundUserInterfaceMessage
{
    public NetEntity Module;

    public ModModuleDeactivateMessage(NetEntity module)
    {
        Module = module;
    }
}
