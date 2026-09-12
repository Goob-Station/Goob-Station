using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.StationRadio.Events;

[Serializable, NetSerializable]
public sealed class StationRadioMediaPlayedEvent : EntityEventArgs
{
    public SoundSpecifier MediaPlayed { get; }
    public StationRadioMediaPlayedEvent(SoundSpecifier Media)
    {
        MediaPlayed = Media;
    }
}
