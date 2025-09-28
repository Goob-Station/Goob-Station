namespace Content.Goobstation.Server.Obsession;

[RegisterComponent]
public sealed partial class ObsessionTargetPhotoComponent : Component
{
    [ViewVariables]
    public List<int> Ids = new();

    [ViewVariables]
    public List<EntityUid> Actors = new();

    public TimeSpan NextUpdate = TimeSpan.Zero;
}
