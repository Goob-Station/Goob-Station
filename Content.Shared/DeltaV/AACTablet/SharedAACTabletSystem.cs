using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.AACTablet;

[Serializable, NetSerializable]
public enum AACTabletKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class AACTabletSendPhraseMessage : BoundUserInterfaceMessage
{
    public string PhraseID;
    public AACTabletSendPhraseMessage(string phraseId)
    {
        PhraseID = phraseId;
    }
}