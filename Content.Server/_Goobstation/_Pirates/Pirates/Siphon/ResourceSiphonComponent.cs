namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

[RegisterComponent, Access(typeof(ResourceSiphonSystem), Other = AccessPermissions.Read)]
public sealed partial class ResourceSiphonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundGamerule;
    [ViewVariables(VVAccess.ReadOnly)] public bool Active = false;

    [DataField] public float CreditsThreshold = 100000f;

    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;

    [DataField] public float DrainRate = 10f;

    [ViewVariables(VVAccess.ReadOnly)] public int ActivationPhase = 0;
    [ViewVariables(VVAccess.ReadOnly)] public float ActivationRewindTime = 3.5f;
    [ViewVariables(VVAccess.ReadOnly)] public float ActivationRewindClock = 3.5f;

    [DataField] public float MaxSignalRange = 250f;
}
