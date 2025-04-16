using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Devil;

[Serializable, NetSerializable]
public sealed class RevivalContractMessage : EuiMessageBase
{
    public readonly bool Accepted;

    public RevivalContractMessage(bool accepted)
    {
        Accepted = accepted;
    }
}
