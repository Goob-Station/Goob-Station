using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

[RegisterComponent]
public sealed partial class DisarmBonusComponent : Component
{
    // A value to modify the disarm success/failure chance
    [DataField]
    public float Bonus { get; set; } = 0.0f; // Default no bonus
}