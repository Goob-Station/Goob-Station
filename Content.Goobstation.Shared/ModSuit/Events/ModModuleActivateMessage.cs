using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed class ModModuleActivateMessage : BoundUserInterfaceMessage
{
    public NetEntity Module;

    public ModModuleActivateMessage(NetEntity module)
    {
        Module = module;
    }
}
