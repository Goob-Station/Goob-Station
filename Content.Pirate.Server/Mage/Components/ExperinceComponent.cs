namespace Content.Pirate.Server.Mage.Components;

[RegisterComponent]
public sealed partial class ExperinceComponent : Component
{
    /// <summary>
    /// The total amount of Experince that the entity has.
    /// Changes based on mob state.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float ExperinceAmount = 10f;
}
