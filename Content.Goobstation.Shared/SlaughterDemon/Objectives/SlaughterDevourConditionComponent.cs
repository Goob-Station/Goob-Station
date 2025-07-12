namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

[RegisterComponent]
public sealed partial class SlaughterDevourConditionComponent : Component
{
    /// <summary>
    /// The amount of devoured crewmembers required
    /// </summary>
    [DataField]
    public int Devour;
}
