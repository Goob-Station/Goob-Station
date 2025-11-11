using System.Numerics;

namespace Content.Goobstation.Server.Photo;

/// <summary>
/// Component used for entities that should not be copied to photo
/// </summary>
[RegisterComponent]
public sealed partial class PhotoCameraIgnoreComponent : Component;
