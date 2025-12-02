using Robust.Shared.Map;

namespace Content.Server._CorvaxGoob.Photo;
public sealed class PhotoCameraTakeImageEvent : EntityEventArgs
{
    public EntityUid Camera { get; }
    public EntityUid User { get; }
    public MapCoordinates PhotoPosition { get; }
    public float Zoom { get; }

    public PhotoCameraTakeImageEvent(EntityUid camera, EntityUid user, MapCoordinates photoPosition, float zoom)
    {
        Camera = camera;
        User = user;
        PhotoPosition = photoPosition;
        Zoom = zoom;
    }
}
