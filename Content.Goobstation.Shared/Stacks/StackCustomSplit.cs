using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Stacks;

[Serializable, NetSerializable]
public sealed class StackCustomSplitAmountMessage : BoundUserInterfaceMessage
{
    public int Amount;

    public StackCustomSplitAmountMessage(int amount)
    {
        Amount = amount;
    }
}
