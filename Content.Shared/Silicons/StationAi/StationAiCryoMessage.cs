using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.StationAi;

[Serializable, NetSerializable]
public sealed class StationAiCryoMessage : EuiMessageBase
{
    public readonly bool Confirmed;
    public StationAiCryoMessage(bool confirmed)
    {
        Confirmed = confirmed;
    }
}
