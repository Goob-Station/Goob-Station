using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Containers.ExtendedContainer;

/// <summary>
/// Manages entities that have a <see cref="ExtendedContainer"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ExtendedContainerComponent : Component
{
    [DataField(readOnly: true)]
    [ViewVariables]
    public string ContainerName = "Extended_container";

    [ViewVariables, NonSerialized]
    public Container Content = default!;

    /// <summary>
    /// How many entities we can store
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Capacity = 50;

    /// <summary>
    /// Whether or not to delete the contents of the container when the entity breaks
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool DeleteContentsOnBreak;

    /// <summary>
    /// Entities we are allowed to insert in the container
    /// </summary>
    [DataField]
    [ViewVariables]
    public EntityWhitelist? InsertWhitelist;

    /// <summary>
    /// Entities we are allowed to remove from the container
    /// </summary>
    [DataField]
    [ViewVariables]
    public EntityWhitelist? RemoveWhitelist;

    [DataField]
    [ViewVariables]
    public SoundSpecifier? InsertSound;

    [DataField]
    [ViewVariables]
    public SoundSpecifier? RemoveSound;
}
