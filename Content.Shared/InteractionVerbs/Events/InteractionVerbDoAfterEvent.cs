using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Events;

[Serializable, NetSerializable]
public sealed partial class InteractionVerbDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public ProtoId<InteractionVerbPrototype>? VerbPrototype;

    [NonSerialized]
    public InteractionArgs? VerbArgs; // Only ever used on the server, it should be fine™. If it ever isn't, move the entire code to server and forget it.

    public InteractionVerbDoAfterEvent(ProtoId<InteractionVerbPrototype>? verbPrototype, InteractionArgs? verbArgs)
    {
        VerbPrototype = verbPrototype;
        VerbArgs = verbArgs;
    }
}
