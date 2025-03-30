namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed class SpaceProofImplantComponent : Component
{
    /// <summary>
    /// Was the entity immune to spacing before being implanted?
    /// </summary>
    [DataField] public bool WasntSpaceProof = false;

    /// <summary>
    /// Did the entity need air before being implanted?
    /// </summary>
    [DataField] public bool NeededAir = false;

}
