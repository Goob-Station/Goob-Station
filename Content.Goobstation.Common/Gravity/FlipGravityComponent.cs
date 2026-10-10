namespace Content.Goobstation.Common.Gravity;

/// <summary>
/// Marker component used to give an entity flipped gravity. Also disable Magboot
/// Eg. Regular walking on space and floating on station
/// </summary>
[RegisterComponent]
public sealed partial class FlipGravityComponent : Component
{
    /// <summary>
    /// How much damage that apply whenever user try to activate magboot
    /// </summary>
    [DataField]
    public int Damage = 1;

    /// <summary>
    /// How long is the stun if the user try to activate magboot
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);
}
