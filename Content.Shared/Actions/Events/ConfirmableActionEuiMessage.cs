using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

[Serializable, NetSerializable]
public sealed class ConfirmableActionEuiMessage(bool accepted) : EuiMessageBase
{
    public bool Accepted = accepted;
}
