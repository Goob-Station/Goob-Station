using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

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
