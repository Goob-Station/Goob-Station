using Content.Shared.DeltaV.QuickPhrase;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.AACTablet;

[Serializable, NetSerializable]
public enum AACTabletKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class AACTabletSendPhraseMessage(ProtoId<QuickPhrasePrototype> phraseId) : BoundUserInterfaceMessage
{
    public ProtoId<QuickPhrasePrototype> PhraseId = phraseId;
}
