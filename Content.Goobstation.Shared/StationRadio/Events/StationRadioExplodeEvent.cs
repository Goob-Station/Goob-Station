using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.StationRadio.Events;

[Serializable, NetSerializable]
public sealed class StationRadioExplodeEvent : EntityEventArgs
{
    public StationRadioExplodeEvent()
    {

    }
}
