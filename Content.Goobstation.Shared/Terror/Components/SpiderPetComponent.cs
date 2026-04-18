using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component to say "Hey I belong to SpiderPetCrownComponent wearer."
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SpiderPetComponent : Component
{
    /// <summary>
    /// The entity wearing the crown that owns this pet.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EntityUid? MasterUid;
}
