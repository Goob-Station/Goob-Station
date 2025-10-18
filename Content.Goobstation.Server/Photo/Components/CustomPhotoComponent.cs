using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Photo;

/// <summary>
/// Component for creating custom photos from exsisting grids
/// </summary>
[RegisterComponent]
public sealed partial class CustomPhotoComponent : Component
{
    /// <summary>
    /// Path to the grid that will be used as photo
    /// </summary>
    [DataField(required: true)]
    public ResPath Photo;
}
