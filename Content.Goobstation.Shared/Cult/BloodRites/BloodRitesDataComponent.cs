using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Cult.BloodRites;

/// <summary>
///     Applies to the action which interacts with a touch spell of type <see cref="BloodRitesGraspComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodRitesDataComponent : Component
{
    [DataField] public float Charges = 0f;
}
