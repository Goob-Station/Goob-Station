using Content.Shared.Verbs;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.Examine;


[Serializable, NetSerializable]
public sealed class ExaminableCharacterInfoMessage : EntityEventArgs
{
    public readonly NetEntity EntityUid;
    public readonly FormattedMessage Message;

    public List<Verb>? Verbs;

    public ExaminableCharacterInfoMessage(NetEntity entityUid, FormattedMessage message, List<Verb>? verbs=null)
    {
        EntityUid = entityUid;
        Message = message;
        Verbs = verbs;
    }
}

