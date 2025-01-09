namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

[RegisterComponent, Access(typeof(ResourceSiphonSystem), Other = AccessPermissions.Read)]
public sealed partial class ResourceSiphonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundGamerule;
    [ViewVariables(VVAccess.ReadOnly)] public bool Active = false;

    [DataField] public float CreditsThreshold = 50000f;
    [DataField] public float SciencePointsThreshold = 50000f;

    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;
    [ViewVariables(VVAccess.ReadWrite)] public float SciencePoints = 0f;

    public int ActivationPhase = 0;
    public float ActivationRewindTime = 3.5f;
    public float ActivationRewindClock = 3.5f;
}
