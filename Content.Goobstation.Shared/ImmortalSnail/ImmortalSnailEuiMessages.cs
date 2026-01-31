using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ImmortalSnail;

[Serializable, NetSerializable]
public enum AcceptImmortalSnailUiButton : byte
{
    Deny,
    Accept,
}

[Serializable, NetSerializable]
public sealed class AcceptImmortalSnailChoiceMessage : EuiMessageBase
{
    public readonly AcceptImmortalSnailUiButton Button;

    public AcceptImmortalSnailChoiceMessage(AcceptImmortalSnailUiButton button)
    {
        Button = button;
    }
}

[Serializable, NetSerializable]
public sealed class AcceptImmortalSnailEuiState : EuiStateBase
{
    public readonly TimeSpan EndTime;

    public AcceptImmortalSnailEuiState(TimeSpan endTime)
    {
        EndTime = endTime;
    }
}
