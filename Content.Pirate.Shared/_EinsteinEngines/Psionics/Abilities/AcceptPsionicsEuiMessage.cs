using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Psionics
{
    [Serializable, NetSerializable]
    public enum AcceptPsionicsUiButton
    {
        Deny,
        Accept,
    }

    [Serializable, NetSerializable]
    public sealed class AcceptPsionicsChoiceMessage : EuiMessageBase
    {
        public readonly AcceptPsionicsUiButton Button;

        public AcceptPsionicsChoiceMessage(AcceptPsionicsUiButton button)
        {
            Button = button;
        }
    }
}
