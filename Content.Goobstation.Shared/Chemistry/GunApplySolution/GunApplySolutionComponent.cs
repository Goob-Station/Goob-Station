namespace Content.Goobstation.Shared.Chemistry.GunApplySolution;

[RegisterComponent]
public sealed partial class GunApplySolutionComponent : Component
{
    [DataField]
    public string SourceSolution = "solution";

    [DataField]
    public string TargetSolution = "ammo";

    [DataField]
    public float Amount = 5f;
}
