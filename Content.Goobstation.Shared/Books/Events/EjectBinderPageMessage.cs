using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class EjectBinderPageMessage : BoundUserInterfaceMessage
{
    public NetEntity Page;

    public EjectBinderPageMessage(NetEntity page)
    {
        Page = page;
    }
}
