using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Containers.ExtendedContainer;

/// <summary>
/// Manages entities that have a <see cref="ExtendedContainer"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ExtendedContainerComponent : Component
{
   // [DataField("containers", readOnly: true)]
    [ViewVariables]
    //public IReadOnlyDictionary<string, ExtendedContainer> Containers = default!;
    public string ContainerName = "Extended_container";

    [ViewVariables, NonSerialized]
    public Container Content = default!;

    /// <summary>
    /// How many entities we can store
    /// </summary>
    [DataField("capacity")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Capacity = 50;

    /// <summary>
    /// Whether or not to delete the contents of the container when the entity breaks
    /// </summary>
    [DataField("deleteContentsOnBreak")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool DeleteContentsOnBreak;

    /// <summary>
    /// Entities we are allowed to insert in the container
    /// </summary>
    [DataField("insertWhitelist")]
    [ViewVariables]
    public EntityWhitelist? InsertWhitelist;

    /// <summary>
    /// Entities we are allowed to remove from the container
    /// </summary>
    [DataField("removeWhitelist")]
    [ViewVariables]
    public EntityWhitelist? RemoveWhitelist;

    [DataField("insertSound")]
    [ViewVariables]
    public SoundSpecifier? InsertSound;

    [DataField("removeSound")]
    [ViewVariables]
    public SoundSpecifier? RemoveSound;

    /*public ExtendedContainer() { }

    public ExtendedContainer(ExtendedContainer other)
    {
        CopyFrom(other);
    }

    public void CopyFrom(ExtendedContainer other)
    {
        // These fields are mutable reference types. But they generally don't get modified, so this should be fine.
        InsertWhitelist = other.InsertWhitelist;
        RemoveWhitelist = other.RemoveWhitelist;
        InsertSound = other.InsertSound;
        RemoveSound = other.RemoveSound;
    }*/
}
