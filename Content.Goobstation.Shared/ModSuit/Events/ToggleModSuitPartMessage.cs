using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed class ToggleModSuitPartMessage : BoundUserInterfaceMessage
{
    public NetEntity PartEntity;

    public ToggleModSuitPartMessage(NetEntity partEntity)
    {
        PartEntity = partEntity;
    }
}
