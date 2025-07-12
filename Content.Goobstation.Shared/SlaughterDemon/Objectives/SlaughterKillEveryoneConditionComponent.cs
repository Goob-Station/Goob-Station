namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

/// <summary>
/// This is used for the objective which is for devouring everyone on the station
/// </summary>
[RegisterComponent]
public sealed partial class SlaughterKillEveryoneConditionComponent : Component
{
    [DataField]
    public int Devoured;
}
