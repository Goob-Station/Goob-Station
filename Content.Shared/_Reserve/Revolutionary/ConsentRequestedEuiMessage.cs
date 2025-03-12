using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Reserve.Revolutionary;

[Serializable, NetSerializable]
public sealed class ConsentRequestedEuiMessage(bool isAccepted) : EuiMessageBase
{
    public readonly bool IsAccepted = isAccepted;
}
