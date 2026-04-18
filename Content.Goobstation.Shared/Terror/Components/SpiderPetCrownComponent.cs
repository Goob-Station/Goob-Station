using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// This is a component that makes it so by wearing something with this component specifically in the head slot, it spawns in a
/// critter (defined through YAML but current default is the spooder) that follows you around. Behaviour of the critter obv depends on the YAML of the entity to spawn.
/// </summary>

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
