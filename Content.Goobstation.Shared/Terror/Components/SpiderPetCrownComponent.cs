using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpiderPetCrownComponent : Component
{
    [DataField]
    public EntProtoId PetPrototype = "NTTerrorSpider";

    /// <summary>
    /// The currently spawned pet, if any.
    /// </summary>
    [DataField]
    public EntityUid? Pet;
}
