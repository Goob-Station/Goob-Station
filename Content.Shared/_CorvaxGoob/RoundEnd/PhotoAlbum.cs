using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.RoundEnd;

[Serializable, NetSerializable]
public sealed class PhotoAlbumEvent : EntityEventArgs
{
    public Dictionary<byte[], string?>? Images { get; }
    public PhotoAlbumEvent(Dictionary<byte[], string?>? images)
    {
        Images = images;
    }
}
