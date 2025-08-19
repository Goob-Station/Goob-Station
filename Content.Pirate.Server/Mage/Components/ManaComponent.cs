namespace Content.Pirate.Server.Mage.Components;

[RegisterComponent]
public sealed partial class ManaComponent : Component
{
    /// <summary>
    /// The total amount of Mana that the entity has.
    /// Changes based on mob state.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float ManaAmount = 0f;
}
