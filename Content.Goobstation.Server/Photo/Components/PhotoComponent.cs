using System.Numerics;

namespace Content.Goobstation.Server.Photo;

[RegisterComponent]
public sealed partial class PhotoComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid SourceEntity;

    public Vector2 Offset;
}
