using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic.Messages;

[Serializable, NetSerializable]
public sealed class FeastOfOwlsMessage(bool accepted) : EuiMessageBase
{
    public readonly bool Accepted = accepted;
}
