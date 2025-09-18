using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithCommandComponent : Component
{
    [DataField]
    public float SearchRange = 4f;

    [DataField(required: true)]
    public EntityWhitelist Blacklist = new();

    /// <summary>
    /// The search range of nearby objects
    /// </summary>
    [DataField]
    public float ObjectSearchRange;

    /// <summary>
    /// Max objects to pick
    /// </summary>
    [DataField]
    public int MaxObjects = 5;

    /// <summary>
    /// The throw speed in which to throw the objects
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;
}
