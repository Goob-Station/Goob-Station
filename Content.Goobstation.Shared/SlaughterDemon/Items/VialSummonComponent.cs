namespace Content.Goobstation.Server.SlaughterDemon.Items;

/// <summary>
/// This is used for tracking objectives
/// </summary>
[RegisterComponent]
public sealed partial class VialSummonComponent : Component
{
    /// <summary>
    ///  The entity who summoned an entity from the vial
    /// </summary>
    [DataField]
    public EntityUid Summoner;

    /// <summary>
    ///  Ensures we get the objective only for that wizard.
    /// </summary>
    [DataField]
    public bool Used;
}
