using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.StationRadio.Events;

[Serializable, NetSerializable]
public sealed class StationRadioMediaStoppedEvent : EntityEventArgs
{
    public StationRadioMediaStoppedEvent()
    {

    }
}
