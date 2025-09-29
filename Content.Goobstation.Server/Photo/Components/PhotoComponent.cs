using System.Numerics;

namespace Content.Goobstation.Server.Photo;

/// <summary>
/// Component containing required info for photo loading
/// </summary>
[RegisterComponent]
public sealed partial class PhotoComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid SourceEntity;

    public Vector2 Offset;
}
