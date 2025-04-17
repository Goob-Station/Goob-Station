using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Devil.UI;

[Serializable, NetSerializable]
public sealed class RevivalContractMessage(bool accepted) : BoundUserInterfaceMessage
{
    public readonly bool Accepted = accepted;
}
