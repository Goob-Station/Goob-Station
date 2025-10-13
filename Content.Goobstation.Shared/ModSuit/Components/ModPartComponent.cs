using Robust.Shared.GameStates;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits;

/// <summary>
///     This component indicates that this clothing is attached to some other entity with a <see
///     cref="ToggleableClothingComponent"/>. When unequipped, this entity should be returned to the entity that it is
///     attached to, rather than being dumped on the floor or something like that. Intended for use with hardsuits and
///     hardsuit helmets.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModPartComponent : Component
{
    /// <summary>
    ///     The Id of the piece of clothing that this entity belongs to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity AttachedUid;

    /// <summary>
    ///     Container ID for clothing that will be replaced with this one
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ClothingContainerId = DefaultClothingContainerId;

    [ViewVariables, NonSerialized]
    public ContainerSlot ClothingContainer = default!;

    public const string DefaultClothingContainerId = "replaced-clothing";
}
