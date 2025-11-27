using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class RadioKeyUnlockedEvent : EntityEventArgs
{
    [DataField(required: true)]
    public List<string> Channels = new();

    public RadioKeyUnlockedEvent(List<string> channels)
    {
        Channels = channels;
    }
};