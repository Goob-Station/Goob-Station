using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Voidwalker.GlassPasser;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class GlassPasserComponent : Component
{
    /// <summary>
    /// Any structures with these tags will not be collided with.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>> PassableTags =
        [ "Window", "Grille" ];

    /// <summary>
    /// The tag for a structure that has been voided and therefore rendered passable.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> VoidedStructureTag = "VoidedStructure";

    /// <summary>
    /// A dictionary containing each entity that has had components added to it, and when it expires.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Dictionary<EntityUid, TimeSpan> EntitiesPassed = new();

    /// <summary>
    /// How long do the components added to an object by passing through them last?
    /// </summary>
    [DataField]
    public TimeSpan EntityPassedAddedComponentsDuration = TimeSpan.FromSeconds(5);
}
