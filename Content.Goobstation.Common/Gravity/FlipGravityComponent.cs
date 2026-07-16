namespace Content.Goobstation.Common.Gravity;

/// <summary>
/// Marker component used to give an entity flipped gravity. Also disable Magboot
/// Eg. Regular walking on space and floating on station
/// </summary>
[RegisterComponent]
public sealed partial class FlipGravityComponent : Component;
