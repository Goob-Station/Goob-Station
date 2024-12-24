namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

[RegisterComponent, Access(typeof(ResourceSiphonSystem), Other = AccessPermissions.Read)]
public sealed partial class ResourceSiphonComponent : Component
{
    [DataField] public float CreditsThreshold = 50000f;
    [DataField] public float SciencePointsThreshold = 50000f;

    public float Credits = 0f;
    public float SciencePoints = 0f;
}
